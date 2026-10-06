(() => {
    "use strict";

    document.addEventListener("DOMContentLoaded", () => {

        /* =========================================================
           STATE
           ========================================================= */

        let hasUnsavedChanges = false;


        /* =========================================================
           UNSAVED STATE
           ========================================================= */

        function setUnsaved(value) {

            hasUnsavedChanges = Boolean(value);

            /*
             * Если на странице есть элементы статуса изменений,
             * обновляем их.
             *
             * Это универсально и не мешает странице Settings,
             * даже если таких элементов нет.
             */

            const status =
                document.querySelector(
                    "[data-settings-unsaved]"
                );

            if (status) {
                status.classList.toggle(
                    "has-changes",
                    hasUnsavedChanges
                );

                status.classList.toggle(
                    "unsaved",
                    hasUnsavedChanges
                );
            }
        }


        /* =========================================================
           TABS
           ========================================================= */

        const tabs =
            document.querySelectorAll(
                ".settings-tab[data-tab]"
            );

        const sections =
            document.querySelectorAll(
                ".settings-tab-content[data-tab-content]"
            );


        function activateTab(tabName) {

            if (!tabName) {
                return;
            }


            tabs.forEach(tab => {

                const active =
                    tab.dataset.tab === tabName;

                tab.classList.toggle(
                    "active",
                    active
                );

                tab.setAttribute(
                    "aria-selected",
                    String(active)
                );
            });


            sections.forEach(section => {

                const active =
                    section.dataset.tabContent === tabName;

                section.classList.toggle(
                    "active",
                    active
                );

                section.hidden =
                    !active;
            });


            /*
             * Сохраняем выбранную вкладку в URL.
             *
             * Например:
             * /Account/Settings#program
             */

            try {

                const url =
                    new URL(
                        window.location.href
                    );

                url.hash =
                    tabName;

                window.history.replaceState(
                    {},
                    "",
                    url
                );

            } catch {
                /*
                 * Если URL по какой-либо причине
                 * не удалось изменить — просто продолжаем.
                 */
            }
        }


        tabs.forEach(tab => {

            tab.addEventListener(
                "click",
                () => {

                    const tabName =
                        tab.dataset.tab;


                    if (!tabName) {
                        return;
                    }


                    /*
                     * На странице Settings больше нет
                     * редактирования учебной нагрузки,
                     * поэтому здесь не требуется предупреждение
                     * о несохранённой нагрузке.
                     */

                    activateTab(tabName);
                }
            );
        });


        /* =========================================================
           INITIAL TAB
           ========================================================= */

        function initializeTabs() {

            let initialTab =
                "profile";


            const hash =
                window.location.hash
                    .replace("#", "")
                    .trim();


            /*
             * Если в URL указана существующая вкладка,
             * открываем её.
             */

            if (
                hash &&
                Array.from(tabs).some(
                    tab =>
                        tab.dataset.tab === hash
                )
            ) {
                initialTab = hash;
            }


            /*
             * Если вкладка Program доступна только админу,
             * обычный пользователь её не увидит,
             * поэтому проверка выше автоматически
             * оставит profile.
             */

            activateTab(initialTab);
        }


        /* =========================================================
           MODALS
           ========================================================= */

        const modalOpenButtons =
            document.querySelectorAll(
                "[data-modal-open]"
            );


        const modalCloseButtons =
            document.querySelectorAll(
                "[data-modal-close]"
            );


        function openModal(modalId) {

            if (!modalId) {
                return;
            }


            const modal =
                document.getElementById(
                    modalId
                );


            if (!modal) {
                return;
            }


            modal.hidden = false;

            modal.classList.add(
                "is-open"
            );


            document.body.classList.add(
                "settings-modal-open"
            );
        }


        function closeModal(modal) {

            if (!modal) {
                return;
            }


            modal.hidden = true;

            modal.classList.remove(
                "is-open"
            );


            /*
             * Убираем блокировку прокрутки,
             * только если других открытых модальных окон нет.
             */

            const anotherOpenedModal =
                document.querySelector(
                    ".settings-modal.is-open"
                );


            if (!anotherOpenedModal) {

                document.body.classList.remove(
                    "settings-modal-open"
                );
            }
        }


        modalOpenButtons.forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    openModal(
                        button.dataset.modalOpen
                    );
                }
            );
        });


        modalCloseButtons.forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    const modal =
                        button.closest(
                            ".settings-modal"
                        );


                    closeModal(modal);
                }
            );
        });


        /* =========================================================
           CLOSE MODAL BY CLICKING BACKDROP
           ========================================================= */

        document
            .querySelectorAll(".settings-modal")
            .forEach(modal => {

                modal.addEventListener(
                    "click",
                    event => {

                        /*
                         * Закрываем окно только при клике
                         * непосредственно по фону.
                         *
                         * Клик внутри modal-content
                         * ничего не делает.
                         */

                        if (
                            event.target === modal
                        ) {
                            closeModal(modal);
                        }
                    }
                );
            });


        /* =========================================================
           ESCAPE
           ========================================================= */

        document.addEventListener(
            "keydown",
            event => {

                if (event.key !== "Escape") {
                    return;
                }


                const openedModal =
                    document.querySelector(
                        ".settings-modal.is-open"
                    );


                if (openedModal) {

                    closeModal(
                        openedModal
                    );
                }
            }
        );


        /* =========================================================
           INITIAL MODAL STATE
           ========================================================= */

        document
            .querySelectorAll(".settings-modal")
            .forEach(modal => {

                /*
                 * Все модальные окна при загрузке закрыты.
                 */

                modal.hidden = true;

                modal.classList.remove(
                    "is-open"
                );
            });


        /*
         * На случай, если класс остался после
         * горячей перезагрузки страницы.
         */

        document.body.classList.remove(
            "settings-modal-open"
        );


        /* =========================================================
           FORM CHANGE DETECTION
           ========================================================= */

        /*
         * Отслеживаем изменения обычных форм Settings.
         *
         * Это НЕ относится к Учебной нагрузке —
         * она теперь находится на отдельной странице.
         */

        const settingsForms =
            document.querySelectorAll(
                ".settings-page form"
            );


        settingsForms.forEach(form => {

            /*
             * Не считаем формы с модальными действиями
             * несохранёнными автоматически.
             *
             * Основные формы профиля/пароля при изменении
             * могут использовать эту логику.
             */

            form.addEventListener(
                "input",
                () => {

                    setUnsaved(true);
                }
            );


            form.addEventListener(
                "change",
                () => {

                    setUnsaved(true);
                }
            );


            form.addEventListener(
                "submit",
                () => {

                    /*
                     * После отправки формы изменения
                     * считаются сохранёнными.
                     */

                    setUnsaved(false);
                }
            );
        });


        /* =========================================================
           BEFORE UNLOAD
           ========================================================= */

        window.addEventListener(
            "beforeunload",
            event => {

                if (!hasUnsavedChanges) {
                    return;
                }


                event.preventDefault();

                /*
                 * Требование браузера для показа
                 * стандартного предупреждения.
                 */

                event.returnValue = "";
            }
        );


        /* =========================================================
           INITIALIZATION
           ========================================================= */

        setUnsaved(false);

        initializeTabs();

    });

})();

