(() => {
    "use strict";

    document.addEventListener("DOMContentLoaded", () => {

        /* =========================================================
           DATA
           ========================================================= */

        const data = window.scheduleSettingsData || {};

        const groups = Array.isArray(data.groups)
            ? data.groups
            : [];

        const subjects = Array.isArray(data.subjects)
            ? data.subjects
            : [];

        const groupSubjects = Array.isArray(data.groupSubjects)
            ? data.groupSubjects
            : [];


        /* =========================================================
           STATE
           ========================================================= */

        let currentGroupId = null;
        let currentLoad = [];
        let hasUnsavedChanges = false;


        /* =========================================================
           ELEMENTS
           ========================================================= */

        const groupSelect =
            document.getElementById("loadGroupSelect");

        const loadContainer =
            document.getElementById("studyLoadContainer");

        const addSubjectArea =
            document.getElementById("addSubjectArea");

        const addSubjectForm =
            document.getElementById("addSubjectForm");

        const addSubjectButton =
            document.getElementById("addSubjectButton");

        const cancelAddSubject =
            document.getElementById("cancelAddSubject");

        const cancelAddSubjectSecondary =
            document.getElementById("cancelAddSubjectSecondary");

        const confirmAddSubject =
            document.getElementById("confirmAddSubject");

        const addSubjectId =
            document.getElementById("addSubjectId");

        const addWeeklyLessons =
            document.getElementById("addWeeklyLessons");

        const saveForm =
            document.getElementById("saveStudyLoadForm");

        const saveButton =
            document.getElementById("saveStudyLoadButton");

        const hiddenInputs =
            document.getElementById("studyLoadHiddenInputs");

        const unsavedStatus =
            document.getElementById("studyLoadUnsavedStatus");

        const statusText =
            document.getElementById("studyLoadStatusText");


        /* =========================================================
           HELPERS
           ========================================================= */

        function escapeHtml(value) {
            if (value === null || value === undefined) {
                return "";
            }

            return String(value)
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");
        }


        function normalizeLessons(value) {
            const number = Number(value);

            if (!Number.isFinite(number)) {
                return 1;
            }

            return Math.max(
                1,
                Math.min(
                    30,
                    Math.round(number)
                )
            );
        }


        function getLessonWord(number) {
            const value = Math.abs(Number(number)) % 100;
            const last = value % 10;

            if (value >= 11 && value <= 19) {
                return "занятий";
            }

            if (last === 1) {
                return "занятие";
            }

            if (last >= 2 && last <= 4) {
                return "занятия";
            }

            return "занятий";
        }


        function getStudentWord(number) {
            const value = Math.abs(Number(number)) % 100;
            const last = value % 10;

            if (value >= 11 && value <= 19) {
                return "студентов";
            }

            if (last === 1) {
                return "студент";
            }

            if (last >= 2 && last <= 4) {
                return "студента";
            }

            return "студентов";
        }


        function getGroup(groupId) {
            return groups.find(
                group =>
                    String(group.id) === String(groupId)
            );
        }


        function getSubject(subjectId) {
            return subjects.find(
                subject =>
                    String(subject.id) === String(subjectId)
            );
        }


        /* =========================================================
           UNSAVED STATE
           ========================================================= */

        function setUnsaved(value) {

            hasUnsavedChanges = Boolean(value);

            if (unsavedStatus) {

                unsavedStatus.classList.toggle(
                    "has-changes",
                    hasUnsavedChanges
                );

                unsavedStatus.classList.toggle(
                    "unsaved",
                    hasUnsavedChanges
                );

                unsavedStatus.style.display = "";
            }

            if (statusText) {

                statusText.textContent =
                    hasUnsavedChanges
                        ? "Есть несохранённые изменения"
                        : "Нет несохранённых изменений";
            }

            if (saveButton) {
                saveButton.disabled =
                    !hasUnsavedChanges;
            }
        }


        /* =========================================================
           PLACEHOLDER
           ========================================================= */

        function renderPlaceholder() {

            if (!loadContainer) {
                return;
            }

            loadContainer.innerHTML = `
                <div class="study-load-placeholder">

                    <div class="study-load-placeholder-icon">
                        👥
                    </div>

                    <strong>
                        Выберите группу
                    </strong>

                    <span>
                        Здесь появятся предметы и количество
                        занятий в неделю.
                    </span>

                </div>
            `;
        }


        /* =========================================================
           RENDER LOAD
           ========================================================= */

        function renderLoad() {

            if (!loadContainer) {
                return;
            }

            if (!currentGroupId) {
                renderPlaceholder();
                return;
            }

            const group = getGroup(currentGroupId);

            if (!group) {
                renderPlaceholder();
                return;
            }

            const courseText =
                group.course
                    ? `${group.course} курс`
                    : "";

            const specialtyText =
                group.specialty || "";

            const studentText =
                group.studentCount
                    ? `${group.studentCount} ${getStudentWord(group.studentCount)}`
                    : "";

            const description = [
                courseText,
                specialtyText,
                studentText
            ]
                .filter(Boolean)
                .join(" • ");


            const total =
                currentLoad.reduce(
                    (sum, item) =>
                        sum + normalizeLessons(item.weeklyLessons),
                    0
                );


            let html = `
                <div class="study-load-workspace">

                    <div class="study-load-workspace-header">

                        <div class="study-load-selected-header">

                            <div class="study-load-label">

                                <strong>
                                    ${escapeHtml(group.name)}
                                </strong>

                                <span>
                                    ${escapeHtml(
                                        description ||
                                        "Учебная нагрузка группы"
                                    )}
                                </span>

                            </div>

                            <div class="study-load-total">
                                ${total} ${getLessonWord(total)} в неделю
                            </div>

                        </div>

                    </div>
            `;


            if (currentLoad.length === 0) {

                html += `
                    <div class="study-load-empty">

                        <div class="study-load-placeholder-icon">
                            📚
                        </div>

                        <strong>
                            Предметы не добавлены
                        </strong>

                        <span>
                            Добавьте предмет, чтобы указать
                            количество занятий в неделю.
                        </span>

                    </div>
                `;

            } else {

                html += `
                    <div class="study-load-list">
                `;


                currentLoad.forEach((item, index) => {

                    const subject =
                        getSubject(item.subjectId);

                    if (!subject) {
                        return;
                    }


                    html += `
                        <div
                            class="study-load-item"
                            data-index="${index}"
                        >

                            <div class="study-load-item-icon">
                                📚
                            </div>


                            <div class="study-load-item-content">

                                <strong>
                                    ${escapeHtml(subject.name)}
                                </strong>

                                <span>
                                    ${
                                        subject.course
                                            ? `${subject.course} курс`
                                            : "Предмет"
                                    }
                                </span>

                            </div>


                            <div class="study-load-item-value">

                                <input
                                    type="number"
                                    class="study-load-lessons-input"
                                    value="${normalizeLessons(item.weeklyLessons)}"
                                    min="1"
                                    max="30"
                                    data-index="${index}"
                                    aria-label="Количество занятий в неделю"
                                />

                            </div>


                            <button
                                type="button"
                                class="study-load-delete-button"
                                data-index="${index}"
                                title="Удалить предмет"
                                aria-label="Удалить предмет"
                            >
                                ×
                            </button>

                        </div>
                    `;
                });


                html += `
                    </div>
                `;
            }


            html += `
                </div>
            `;


            loadContainer.innerHTML = html;

            bindLoadEvents();
        }


        /* =========================================================
           UPDATE ONLY TOTAL
           ========================================================= */

        function updateTotal() {

            const total =
                currentLoad.reduce(
                    (sum, item) =>
                        sum + normalizeLessons(item.weeklyLessons),
                    0
                );

            const totalElement =
                loadContainer?.querySelector(
                    ".study-load-total"
                );

            if (totalElement) {
                totalElement.textContent =
                    `${total} ${getLessonWord(total)} в неделю`;
            }
        }


        /* =========================================================
           LOAD EVENTS
           ========================================================= */

        function bindLoadEvents() {

            if (!loadContainer) {
                return;
            }


            const lessonInputs =
                loadContainer.querySelectorAll(
                    ".study-load-lessons-input"
                );


            lessonInputs.forEach(input => {

                input.addEventListener(
                    "input",
                    () => {

                        const index =
                            Number(input.dataset.index);

                        if (
                            !Number.isInteger(index) ||
                            !currentLoad[index]
                        ) {
                            return;
                        }


                        let value =
                            Number(input.value);


                        if (!Number.isFinite(value)) {
                            value = 1;
                        }


                        value = Math.max(
                            1,
                            Math.min(
                                30,
                                Math.round(value)
                            )
                        );


                        currentLoad[index].weeklyLessons =
                            value;


                        setUnsaved(true);

                        updateTotal();
                    }
                );


                input.addEventListener(
                    "blur",
                    () => {

                        const index =
                            Number(input.dataset.index);

                        if (
                            !Number.isInteger(index) ||
                            !currentLoad[index]
                        ) {
                            return;
                        }


                        const value =
                            normalizeLessons(
                                input.value
                            );


                        currentLoad[index].weeklyLessons =
                            value;

                        input.value = value;

                        updateTotal();
                    }
                );
            });


            const deleteButtons =
                loadContainer.querySelectorAll(
                    ".study-load-delete-button"
                );


            deleteButtons.forEach(button => {

                button.addEventListener(
                    "click",
                    () => {

                        const index =
                            Number(button.dataset.index);

                        if (
                            !Number.isInteger(index) ||
                            !currentLoad[index]
                        ) {
                            return;
                        }


                        const subject =
                            getSubject(
                                currentLoad[index].subjectId
                            );


                        const confirmed =
                            window.confirm(
                                `Удалить предмет "${subject?.name ?? "предмет"}" из учебной нагрузки?`
                            );


                        if (!confirmed) {
                            return;
                        }


                        currentLoad.splice(
                            index,
                            1
                        );


                        renderLoad();
                        updateAvailableSubjects();
                        setUnsaved(true);
                    }
                );
            });
        }


        /* =========================================================
           AVAILABLE SUBJECTS
           ========================================================= */

        function updateAvailableSubjects() {

            if (!addSubjectId) {
                return;
            }


            const selected =
                new Set(
                    currentLoad.map(
                        item =>
                            String(item.subjectId)
                    )
                );


            const group =
                getGroup(currentGroupId);


            const available =
                subjects
                    .filter(subject => {

                        if (
                            group &&
                            group.course &&
                            subject.course
                        ) {
                            return Number(subject.course) ===
                                Number(group.course);
                        }

                        return true;
                    })
                    .filter(subject =>
                        !selected.has(
                            String(subject.id)
                        )
                    )
                    .sort(
                        (a, b) =>
                            String(a.name)
                                .localeCompare(
                                    String(b.name),
                                    "ru"
                                )
                    );


            addSubjectId.innerHTML =
                `<option value="">Выберите предмет</option>`;


            available.forEach(subject => {

                const option =
                    document.createElement("option");

                option.value =
                    subject.id;

                option.textContent =
                    subject.name;

                addSubjectId.appendChild(
                    option
                );
            });


            addSubjectId.disabled =
                available.length === 0;


            if (addSubjectButton) {

                addSubjectButton.disabled =
                    available.length === 0;
            }
        }


        /* =========================================================
           LOAD GROUP
           ========================================================= */

        function loadGroup(groupId) {

            if (!groupId) {

                currentGroupId = null;
                currentLoad = [];

                renderPlaceholder();
                updateAvailableSubjects();
                setUnsaved(false);

                if (addSubjectArea) {
                    addSubjectArea.style.display = "none";
                }

                if (addSubjectForm) {
                    addSubjectForm.style.display = "none";
                }

                return;
            }


            const group =
                getGroup(groupId);


            if (!group) {
                return;
            }


            currentGroupId =
                group.id;


            currentLoad =
                groupSubjects
                    .filter(
                        item =>
                            String(item.groupId) ===
                            String(group.id)
                    )
                    .map(item => ({
                        id: item.id,
                        groupId: item.groupId,
                        subjectId: item.subjectId,
                        weeklyLessons:
                            normalizeLessons(
                                item.weeklyLessons
                            )
                    }));


            renderLoad();
            updateAvailableSubjects();
            setUnsaved(false);


            if (addSubjectArea) {
                addSubjectArea.style.display = "";
            }


            if (addSubjectForm) {
                addSubjectForm.style.display = "none";
            }
        }


        /* =========================================================
           GROUP SELECT
           ========================================================= */

        if (groupSelect) {

            groupSelect.addEventListener(
                "change",
                () => {

                    if (
                        hasUnsavedChanges &&
                        currentGroupId !== null
                    ) {

                        const confirmed =
                            window.confirm(
                                "Есть несохранённые изменения. " +
                                "Переключиться на другую группу без сохранения?"
                            );


                        if (!confirmed) {

                            groupSelect.value =
                                currentGroupId;

                            return;
                        }
                    }


                    loadGroup(
                        groupSelect.value
                    );
                }
            );
        }


        /* =========================================================
           ADD SUBJECT FORM
           ========================================================= */

        function openAddSubjectForm() {

            if (!currentGroupId) {

                window.alert(
                    "Сначала выберите группу."
                );

                return;
            }


            updateAvailableSubjects();


            if (addSubjectForm) {
                addSubjectForm.style.display = "";
            }


            if (addSubjectArea) {
                addSubjectArea.style.display = "none";
            }


            if (addSubjectId) {
                addSubjectId.value = "";
                addSubjectId.focus();
            }


            if (addWeeklyLessons) {
                addWeeklyLessons.value = "2";
            }
        }


        function closeAddSubjectForm() {

            if (addSubjectForm) {
                addSubjectForm.style.display = "none";
            }


            if (addSubjectArea) {
                addSubjectArea.style.display = "";
            }
        }


        if (addSubjectButton) {

            addSubjectButton.addEventListener(
                "click",
                openAddSubjectForm
            );
        }


        if (cancelAddSubject) {

            cancelAddSubject.addEventListener(
                "click",
                closeAddSubjectForm
            );
        }


        if (cancelAddSubjectSecondary) {

            cancelAddSubjectSecondary.addEventListener(
                "click",
                closeAddSubjectForm
            );
        }


        if (confirmAddSubject) {

            confirmAddSubject.addEventListener(
                "click",
                () => {

                    if (!currentGroupId) {

                        window.alert(
                            "Сначала выберите группу."
                        );

                        return;
                    }


                    const subjectId =
                        addSubjectId
                            ? addSubjectId.value
                            : "";


                    if (!subjectId) {

                        window.alert(
                            "Выберите предмет."
                        );

                        return;
                    }


                    const subject =
                        getSubject(subjectId);


                    if (!subject) {
                        return;
                    }


                    const alreadyExists =
                        currentLoad.some(
                            item =>
                                String(item.subjectId) ===
                                String(subjectId)
                        );


                    if (alreadyExists) {

                        window.alert(
                            "Этот предмет уже добавлен."
                        );

                        return;
                    }


                    const lessons =
                        normalizeLessons(
                            addWeeklyLessons
                                ? addWeeklyLessons.value
                                : 2
                        );


                    currentLoad.push({
                        id: null,
                        groupId: currentGroupId,
                        subjectId: subject.id,
                        weeklyLessons: lessons
                    });


                    renderLoad();
                    updateAvailableSubjects();
                    setUnsaved(true);

                    closeAddSubjectForm();
                }
            );
        }


        /* =========================================================
           SAVE STUDY LOAD
           ========================================================= */

        if (saveForm) {

            saveForm.addEventListener(
                "submit",
                event => {

                    if (!currentGroupId) {

                        event.preventDefault();

                        window.alert(
                            "Сначала выберите группу."
                        );

                        return;
                    }


                    if (!hiddenInputs) {

                        event.preventDefault();

                        return;
                    }


                    hiddenInputs.innerHTML = "";


                    /* GROUP ID */

                    const groupInput =
                        document.createElement("input");

                    groupInput.type = "hidden";
                    groupInput.name = "groupId";
                    groupInput.value =
                        currentGroupId;

                    hiddenInputs.appendChild(
                        groupInput
                    );


                    /* ITEMS */

                    currentLoad.forEach(
                        (item, index) => {

                            const subjectInput =
                                document.createElement("input");

                            subjectInput.type = "hidden";

                            subjectInput.name =
                                `items[${index}].SubjectId`;

                            subjectInput.value =
                                item.subjectId;

                            hiddenInputs.appendChild(
                                subjectInput
                            );


                            const lessonsInput =
                                document.createElement("input");

                            lessonsInput.type = "hidden";

                            lessonsInput.name =
                                `items[${index}].WeeklyLessons`;

                            lessonsInput.value =
                                normalizeLessons(
                                    item.weeklyLessons
                                );

                            hiddenInputs.appendChild(
                                lessonsInput
                            );


                            if (
                                item.id !== null &&
                                item.id !== undefined &&
                                item.id !== ""
                            ) {

                                const idInput =
                                    document.createElement("input");

                                idInput.type = "hidden";

                                idInput.name =
                                    `items[${index}].Id`;

                                idInput.value =
                                    item.id;

                                hiddenInputs.appendChild(
                                    idInput
                                );
                            }
                        }
                    );


                    setUnsaved(false);
                }
            );
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
                    section.dataset.tabContent ===
                    tabName;

                section.classList.toggle(
                    "active",
                    active
                );

                section.hidden =
                    !active;
            });


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
                // Игнорируем ошибку URL.
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


                    if (
                        hasUnsavedChanges &&
                        currentGroupId !== null &&
                        tabName !== "program"
                    ) {

                        const confirmed =
                            window.confirm(
                                "Есть несохранённые изменения в учебной нагрузке. " +
                                "Перейти в другой раздел без сохранения?"
                            );


                        if (!confirmed) {
                            return;
                        }


                        setUnsaved(false);
                    }


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


            if (
                hash &&
                Array.from(tabs).some(
                    tab =>
                        tab.dataset.tab === hash
                )
            ) {
                initialTab = hash;
            }


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


            document.body.classList.remove(
                "settings-modal-open"
            );
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
                    closeModal(openedModal);
                }
            }
        );


        /* =========================================================
           INITIAL MODAL STATE
           ========================================================= */

        document
            .querySelectorAll(".settings-modal")
            .forEach(modal => {

                modal.hidden = true;

                modal.classList.remove(
                    "is-open"
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
                event.returnValue = "";
            }
        );


        /* =========================================================
           INITIALIZATION
           ========================================================= */

        renderPlaceholder();

        setUnsaved(false);

        initializeTabs();

    });

})();