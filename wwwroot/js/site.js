(() => {
    "use strict";

    let activeDialog = null;
    let dialogFocus = null;
    let isolatedNodes = [];
    const focusableSelector = "a[href], button:not(:disabled), input:not(:disabled):not([type='hidden']), select:not(:disabled), textarea:not(:disabled), [tabindex]:not([tabindex='-1'])";

    function focusableElements(container) {
        return Array.from(container.querySelectorAll(focusableSelector))
            .filter(element => element.getClientRects().length && !element.closest("[inert]"));
    }

    function trapTab(event, container) {
        if (event.key !== "Tab") return;
        const elements = focusableElements(container);
        if (!elements.length) {
            event.preventDefault();
            container.focus();
            return;
        }
        const first = elements[0];
        const last = elements[elements.length - 1];
        if (event.shiftKey && (document.activeElement === first || !container.contains(document.activeElement))) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && (document.activeElement === last || !container.contains(document.activeElement))) {
            event.preventDefault();
            first.focus();
        }
    }

    function restoreBackground() {
        isolatedNodes.forEach(([element, wasInert]) => { element.inert = wasInert; });
        isolatedNodes = [];
    }

    // Isolate siblings at each level, never an ancestor containing the dialog.
    function isolateBackground(root) {
        let current = root;
        while (current.parentElement && current.parentElement !== document.documentElement) {
            Array.from(current.parentElement.children).forEach(sibling => {
                if (sibling !== current && !["SCRIPT", "STYLE", "LINK"].includes(sibling.tagName)) {
                    isolatedNodes.push([sibling, sibling.inert]);
                    sibling.inert = true;
                }
            });
            current = current.parentElement;
        }
    }

    function focusDialog(root, options = {}) {
        if (!root) return;
        const dialog = root.matches("[role='dialog']") ? root : root.querySelector("[role='dialog']");
        if (!dialog) return;
        restoreBackground();
        activeDialog = { root, dialog };
        dialogFocus = options.returnFocus || document.activeElement;
        isolateBackground(root);
        document.body.classList.add("ui-modal-open");
        dialog.tabIndex = -1;
        (options.initialFocus || focusableElements(dialog)[0] || dialog).focus();
    }

    function focusWarning() {
        const warnings = Array.from(document.querySelectorAll(".warning-overlay"));
        const root = warnings[warnings.length - 1];
        if (!root) return;
        const cancel = root.querySelector(".warning-cancel, .validation-error-button");
        const field = document.getElementById(root.id.startsWith("classroom") ? "ClassroomId" : "TeacherId");
        focusDialog(root, { initialFocus: cancel, returnFocus: field });
    }

    function releaseDialog(root) {
        if (activeDialog?.root !== root) return;
        const returnFocus = dialogFocus;
        restoreBackground();
        activeDialog = null;
        dialogFocus = null;
        document.body.classList.remove("ui-modal-open");
        if (document.querySelector(".warning-overlay")) {
            focusWarning();
        } else if (returnFocus?.isConnected) {
            returnFocus.focus();
        }
    }

    // Existing page handlers retain ownership of open/close, Escape and submit.
    window.scheduleUi = { focusDialog, releaseDialog };
    document.addEventListener("keydown", event => {
        if (activeDialog) trapTab(event, activeDialog.dialog);
    });
    document.addEventListener("focusin", event => {
        if (activeDialog && !activeDialog.dialog.contains(event.target)) {
            (focusableElements(activeDialog.dialog)[0] || activeDialog.dialog).focus();
        }
    });

    document.addEventListener("DOMContentLoaded", () => {
        const shell = document.querySelector(".app-shell");
        const sidebar = document.getElementById("appSidebar");
        const appMain = document.querySelector(".app-main");
        const openButton = document.querySelector("[data-sidebar-open]");
        const narrowScreen = window.matchMedia("(max-width: 991px)");
        let menuReturnFocus = null;

        function closeSidebar(restoreFocus = true) {
            shell.classList.remove("sidebar-open");
            sidebar.inert = narrowScreen.matches;
            sidebar.removeAttribute("role");
            sidebar.removeAttribute("aria-modal");
            appMain.inert = false;
            openButton?.setAttribute("aria-expanded", "false");
            document.body.classList.toggle("ui-modal-open", Boolean(activeDialog));
            if (restoreFocus && menuReturnFocus?.isConnected) menuReturnFocus.focus();
            menuReturnFocus = null;
        }

        if (shell && sidebar && appMain) {
            sidebar.inert = narrowScreen.matches;
            document.querySelectorAll(".sidebar-link.active").forEach(link => link.setAttribute("aria-current", "page"));
            openButton?.addEventListener("click", () => {
                menuReturnFocus = document.activeElement;
                shell.classList.add("sidebar-open");
                sidebar.inert = false;
                sidebar.setAttribute("role", "dialog");
                sidebar.setAttribute("aria-modal", "true");
                appMain.inert = true;
                openButton.setAttribute("aria-expanded", "true");
                document.body.classList.add("ui-modal-open");
                sidebar.querySelector(".sidebar-close").focus();
            });
            document.querySelectorAll("[data-sidebar-close]").forEach(button => button.addEventListener("click", () => closeSidebar()));
            document.addEventListener("keydown", event => {
                if (!shell.classList.contains("sidebar-open")) return;
                if (event.key === "Escape") {
                    event.preventDefault();
                    closeSidebar();
                } else {
                    trapTab(event, sidebar);
                }
            });
            narrowScreen.addEventListener("change", () => closeSidebar(false));
        }

        // Keep the existing Razor category mapping; normalize its decorative glyphs.
        const categoryIcons = { "⌨": "keyboard", "⚗": "eyedropper", "▤": "journal-text", "⚒": "tools", "◉": "dribbble", "▦": "grid", "✦": "palette", "◫": "easel", "⌂": "door-open" };
        document.querySelectorAll(".classroom-option-icon").forEach(element => {
            const iconName = categoryIcons[element.textContent.trim()];
            if (!iconName) return;
            const icon = document.createElement("i");
            icon.className = "bi bi-" + iconName;
            icon.setAttribute("aria-hidden", "true");
            element.replaceChildren(icon);
        });

        document.querySelectorAll(".warning-overlay").forEach(root => {
            const dialog = root.firstElementChild;
            const heading = dialog.querySelector("h2");
            dialog.setAttribute("role", "dialog");
            dialog.setAttribute("aria-modal", "true");
            if (heading) {
                heading.id = root.id + "Title";
                dialog.setAttribute("aria-labelledby", heading.id);
            }
        });
        focusWarning();
        document.addEventListener("keydown", event => {
            if (event.key !== "Escape" || !activeDialog?.root.matches(".warning-overlay")) return;
            event.preventDefault();
            activeDialog.root.querySelector(".warning-cancel, .validation-error-button")?.click();
        });
    });
})();
