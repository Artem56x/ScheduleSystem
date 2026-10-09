(() => {
    "use strict";

    document.addEventListener("DOMContentLoaded", () => {

        const data = window.scheduleStudyLoadData || {};

        const groups = Array.isArray(data.groups)
            ? data.groups
            : [];

        const subjects = Array.isArray(data.subjects)
            ? data.subjects
            : [];

        const groupSubjects = Array.isArray(data.groupSubjects)
            ? data.groupSubjects
            : [];


        // =========================================================
        // ELEMENTS
        // =========================================================

        const groupSelect =
            document.getElementById("loadGroupSelect");

        const container =
            document.getElementById("studyLoadContainer");

        const addSubjectArea =
            document.getElementById("addSubjectArea");

        const addSubjectButton =
            document.getElementById("addSubjectButton");

        const addSubjectForm =
            document.getElementById("addSubjectForm");

        const cancelAddSubject =
            document.getElementById("cancelAddSubject");

        const cancelAddSubjectSecondary =
            document.getElementById("cancelAddSubjectSecondary");

        const addSubjectId =
            document.getElementById("addSubjectId");

        const addWeeklyLessons =
            document.getElementById("addWeeklyLessons");

        const confirmAddSubject =
            document.getElementById("confirmAddSubject");

        const saveForm =
            document.getElementById("saveStudyLoadForm");

        const hiddenInputs =
            document.getElementById("studyLoadHiddenInputs");

        const saveButton =
            document.getElementById("saveStudyLoadButton");

        const statusDot =
            document.getElementById("studyLoadUnsavedStatus");

        const statusText =
            document.getElementById("studyLoadStatusText");


        if (!groupSelect || !container || !saveForm) {

            console.error(
                "StudyLoad: необходимые элементы страницы не найдены."
            );

            return;
        }


        // =========================================================
        // STATE
        // =========================================================

        let currentGroupId = null;

        let currentLoad = [];

        let hasUnsavedChanges = false;


        // =========================================================
        // HELPERS
        // =========================================================

        function showStudyLoadError(message) {
            const error = document.getElementById("studyLoadClientError");
            error.textContent = message;
            error.focus();
        }

        function escapeHtml(value) {

            return String(value ?? "")
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");
        }


        function normalizeLessons(value) {

            const number = Number.parseInt(value, 10);

            if (Number.isNaN(number)) {
                return 1;
            }

            return Math.min(
                20,
                Math.max(1, number)
            );
        }


        function getLessonWord(number) {

            const value = Math.abs(number) % 100;
            const last = value % 10;

            if (value >= 11 && value <= 14) {
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

            const value = Math.abs(number) % 100;
            const last = value % 10;

            if (value >= 11 && value <= 14) {
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


        function getGroup(id) {

            return groups.find(
                group => Number(group.id) === Number(id)
            );
        }


        function getSubject(id) {

            return subjects.find(
                subject => Number(subject.id) === Number(id)
            );
        }


        function getTotalLessons() {

            return currentLoad.reduce(
                (total, item) =>
                    total + normalizeLessons(item.weeklyLessons),
                0
            );
        }


        // =========================================================
        // UNSAVED STATE
        // =========================================================

        function setUnsaved(value) {

            hasUnsavedChanges = Boolean(value);

            if (saveButton) {

                saveButton.disabled =
                    !hasUnsavedChanges ||
                    !currentGroupId;
            }

            if (statusDot) {

                statusDot.classList.toggle(
                    "unsaved",
                    hasUnsavedChanges
                );

                statusDot.classList.toggle(
                    "saved",
                    !hasUnsavedChanges
                );
            }

            if (statusText) {

                statusText.textContent =
                    hasUnsavedChanges
                        ? "Есть несохранённые изменения"
                        : "Изменения не внесены";
            }
        }


        // =========================================================
        // PLACEHOLDER
        // =========================================================

        function renderPlaceholder() {

            container.innerHTML = `
                <div class="study-load-placeholder">

                    <div class="study-load-placeholder-icon">
                        <i class="bi bi-clipboard" aria-hidden="true"></i>
                    </div>

                    <h3>Группа не выбрана</h3>

                    <p>
                        Выберите учебную группу выше,
                        чтобы настроить её учебную нагрузку.
                    </p>

                </div>
            `;

            if (addSubjectArea) {
                addSubjectArea.hidden = true;
            }

            setUnsaved(false);
        }


        // =========================================================
        // RENDER LOAD
        // =========================================================

        function renderLoad() {

            const group = getGroup(currentGroupId);

            if (!group) {
                renderPlaceholder();
                return;
            }

            const totalLessons =
                getTotalLessons();

            let itemsHtml = "";


            if (currentLoad.length === 0) {

                itemsHtml = `
                    <div class="study-load-empty">

                        <div class="study-load-empty-icon">
                            <i class="bi bi-journal-bookmark" aria-hidden="true"></i>
                        </div>

                        <strong>
                            Предметы ещё не добавлены
                        </strong>

                        <span>
                            Нажмите «Добавить предмет»,
                            чтобы создать учебную нагрузку.
                        </span>

                    </div>
                `;

            } else {

                itemsHtml = currentLoad
                    .map((item, index) => {

                        const subject =
                            getSubject(item.subjectId);

                        if (!subject) {
                            return "";
                        }

                        return `
                            <div
                                class="study-load-item"
                                data-index="${index}"
                            >

                                <div class="study-load-item-icon">
                                    <i class="bi bi-journal-bookmark" aria-hidden="true"></i>
                                </div>

                                <div class="study-load-item-content">

                                    <strong>
                                        ${escapeHtml(subject.name)}
                                    </strong>

                                    <span>
                                        ${subject.course} курс
                                    </span>

                                </div>

                                <div class="study-load-item-value">

                                    <input
                                        class="study-load-lessons-input"
                                        type="number"
                                        min="1"
                                        max="20"
                                        value="${normalizeLessons(item.weeklyLessons)}"
                                        data-index="${index}"
                                        aria-label="Количество занятий в неделю: ${escapeHtml(subject.name)}"
                                    />

                                </div>

                                <button
                                    type="button"
                                    class="study-load-delete-button ui-button ui-icon-button"
                                    data-index="${index}"
                                    title="Удалить предмет"
                                    aria-label="Удалить предмет ${escapeHtml(subject.name)}"
                                >
                                    <i class="bi bi-x-lg" aria-hidden="true"></i>
                                </button>

                            </div>
                        `;
                    })
                    .join("");
            }


            container.innerHTML = `
                <div class="study-load-workspace">

                    <div class="study-load-workspace-header">

                        <div class="study-load-selected-header">

                            <div class="study-load-label">

                                <strong>
                                    ${escapeHtml(group.name)}
                                </strong>

                                <span>
                                    ${escapeHtml(group.specialty || "")}
                                    · ${group.course} курс
                                    · ${group.studentCount}
                                    ${getStudentWord(group.studentCount)}
                                </span>

                            </div>

                            <div class="study-load-total">

                                ${totalLessons}
                                ${getLessonWord(totalLessons)}
                                в неделю

                            </div>

                        </div>

                    </div>

                    <div class="study-load-list">
                        ${itemsHtml}
                    </div>

                </div>
            `;


            if (addSubjectArea) {
                addSubjectArea.hidden = false;
            }

            attachLoadEvents();

            updateAvailableSubjects();
        }


        // =========================================================
        // UPDATE TOTAL
        // =========================================================

        function updateTotal() {

            const total =
                getTotalLessons();

            const totalElement =
                container.querySelector(
                    ".study-load-total"
                );

            if (totalElement) {

                totalElement.textContent =
                    `${total} ${getLessonWord(total)} в неделю`;
            }
        }


        // =========================================================
        // LOAD EVENTS
        // =========================================================

        function attachLoadEvents() {

            const lessonInputs =
                container.querySelectorAll(
                    ".study-load-lessons-input"
                );


            lessonInputs.forEach(input => {

                input.addEventListener(
                    "input",
                    () => {

                        const index =
                            Number(input.dataset.index);

                        if (
                            Number.isNaN(index) ||
                            !currentLoad[index]
                        ) {
                            return;
                        }

                        currentLoad[index].weeklyLessons =
                            normalizeLessons(input.value);

                        setUnsaved(true);

                        updateTotal();
                    }
                );


                input.addEventListener(
                    "blur",
                    () => {

                        input.value =
                            normalizeLessons(input.value);

                        const index =
                            Number(input.dataset.index);

                        if (
                            !Number.isNaN(index) &&
                            currentLoad[index]
                        ) {

                            currentLoad[index].weeklyLessons =
                                normalizeLessons(input.value);
                        }

                        updateTotal();
                    }
                );
            });


            const deleteButtons =
                container.querySelectorAll(
                    ".study-load-delete-button"
                );


            deleteButtons.forEach(button => {

                button.addEventListener(
                    "click",
                    () => {

                        const index =
                            Number(button.dataset.index);

                        if (
                            Number.isNaN(index) ||
                            !currentLoad[index]
                        ) {
                            return;
                        }

                        currentLoad.splice(index, 1);

                        renderLoad();

                        setUnsaved(true);
                    }
                );
            });
        }


        // =========================================================
        // AVAILABLE SUBJECTS
        // =========================================================

        function updateAvailableSubjects() {

            if (!addSubjectId) {
                return;
            }


            addSubjectId.innerHTML = `
                <option value="">
                    Выберите предмет...
                </option>
            `;


            if (!currentGroupId) {

                addSubjectId.disabled = true;

                return;
            }


            const group =
                getGroup(currentGroupId);


            if (!group) {

                addSubjectId.disabled = true;

                return;
            }


            const existingSubjectIds =
                new Set(
                    currentLoad.map(
                        item => Number(item.subjectId)
                    )
                );


            const availableSubjects =
                subjects
                    .filter(subject =>
                        Number(subject.course) ===
                        Number(group.course)
                    )
                    .filter(subject =>
                        !existingSubjectIds.has(
                            Number(subject.id)
                        )
                    )
                    .sort((a, b) =>
                        String(a.name).localeCompare(
                            String(b.name),
                            "ru"
                        )
                    );


            availableSubjects.forEach(subject => {

                const option =
                    document.createElement("option");

                option.value =
                    subject.id;

                option.textContent =
                    subject.name;

                addSubjectId.appendChild(option);
            });


            addSubjectId.disabled =
                availableSubjects.length === 0;


            if (availableSubjects.length === 0) {

                addSubjectId.innerHTML = `
                    <option value="">
                        Все предметы уже добавлены
                    </option>
                `;
            }
        }


        // =========================================================
        // OPEN ADD FORM
        // =========================================================

        function openAddForm() {

            if (!currentGroupId) {
                return;
            }

            updateAvailableSubjects();


            if (addSubjectForm) {
                addSubjectForm.hidden = false;
            }


            if (addWeeklyLessons) {

                addWeeklyLessons.value = 2;

                setTimeout(() => {
                    addWeeklyLessons.focus();
                }, 50);
            }
        }


        // =========================================================
        // CLOSE ADD FORM
        // =========================================================

        function closeAddForm() {

            if (addSubjectForm) {
                addSubjectForm.hidden = true;
                addSubjectButton?.focus();
            }

            if (addSubjectId) {
                addSubjectId.value = "";
            }

            if (addWeeklyLessons) {
                addWeeklyLessons.value = 2;
            }
        }


        // =========================================================
        // ADD SUBJECT
        // =========================================================

        function addSubject() {
            document.getElementById("studyLoadClientError").textContent = "";

            if (!currentGroupId) {
                return;
            }


            const subjectId =
                Number(addSubjectId?.value);


            const weeklyLessons =
                normalizeLessons(
                    addWeeklyLessons?.value
                );


            if (!subjectId) {

                showStudyLoadError("Выберите предмет.");

                return;
            }


            const subject =
                getSubject(subjectId);


            if (!subject) {

                showStudyLoadError("Выбранный предмет не найден.");

                return;
            }


            const group =
                getGroup(currentGroupId);


            if (!group) {
                return;
            }


            if (
                Number(subject.course) !==
                Number(group.course)
            ) {

                showStudyLoadError(
                    "Предмет и группа должны быть одного курса."
                );

                return;
            }


            const alreadyExists =
                currentLoad.some(
                    item =>
                        Number(item.subjectId) ===
                        subjectId
                );


            if (alreadyExists) {

                showStudyLoadError(
                    "Этот предмет уже добавлен."
                );

                return;
            }


            currentLoad.push({

                id: null,

                groupId: currentGroupId,

                subjectId: subjectId,

                weeklyLessons: weeklyLessons

            });


            renderLoad();

            setUnsaved(true);

            closeAddForm();
        }


        // =========================================================
        // GROUP CHANGE
        // =========================================================

        groupSelect.addEventListener(
            "change",
            () => {

                const value =
                    Number(groupSelect.value);


                if (!value) {

                    currentGroupId = null;

                    currentLoad = [];

                    renderPlaceholder();

                    return;
                }


                currentGroupId = value;


                currentLoad =
                    groupSubjects
                        .filter(
                            item =>
                                Number(item.groupId) ===
                                Number(currentGroupId)
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


                closeAddForm();

                renderLoad();

                setUnsaved(false);
            }
        );


        // =========================================================
        // ADD BUTTON
        // =========================================================

        if (addSubjectButton) {

            addSubjectButton.addEventListener(
                "click",
                openAddForm
            );
        }


        // =========================================================
        // CANCEL
        // =========================================================

        if (cancelAddSubject) {

            cancelAddSubject.addEventListener(
                "click",
                closeAddForm
            );
        }


        if (cancelAddSubjectSecondary) {

            cancelAddSubjectSecondary.addEventListener(
                "click",
                closeAddForm
            );
        }


        // =========================================================
        // CONFIRM ADD
        // =========================================================

        if (confirmAddSubject) {

            confirmAddSubject.addEventListener(
                "click",
                addSubject
            );
        }


        // =========================================================
        // SAVE
        // =========================================================

        saveForm.addEventListener(
            "submit",
            event => {

                if (!currentGroupId) {

                    event.preventDefault();

                    showStudyLoadError(
                        "Сначала выберите учебную группу."
                    );

                    return;
                }


                if (!hasUnsavedChanges) {

                    event.preventDefault();

                    return;
                }


                if (!hiddenInputs) {
                    return;
                }


                hiddenInputs.innerHTML = "";


                // -------------------------------------------------
                // GROUP ID
                // -------------------------------------------------

                const groupInput =
                    document.createElement("input");

                groupInput.type = "hidden";
                groupInput.name = "groupId";
                groupInput.value = currentGroupId;

                hiddenInputs.appendChild(
                    groupInput
                );


                // -------------------------------------------------
                // ITEMS
                // -------------------------------------------------

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
                            Number(item.id) > 0
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


                saveButton.disabled = true;

                statusText.textContent =
                    "Сохранение...";

                statusDot.classList.remove(
                    "unsaved"
                );

                statusDot.classList.add(
                    "saved"
                );
            }
        );


        // =========================================================
        // BEFORE UNLOAD
        // =========================================================

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


        // =========================================================
        // INITIAL STATE
        // =========================================================

        renderPlaceholder();

    });

})();

