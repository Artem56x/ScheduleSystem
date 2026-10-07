document.addEventListener("DOMContentLoaded", function () {

    /* =====================================================
       ELEMENTS
       ===================================================== */

    const printScheduleButton =
        document.getElementById("printScheduleButton");

    const searchInput =
        document.getElementById("scheduleSearch");

    const groupFilter =
        document.getElementById("filterGroup");

    const teacherFilter =
        document.getElementById("filterTeacher");

    const subjectFilter =
        document.getElementById("filterSubject");

    const classroomFilter =
        document.getElementById("filterClassroom");

    const dayFilter =
        document.getElementById("filterDay");

    const timeFromFilter =
        document.getElementById("filterTimeFrom");

    const timeToFilter =
        document.getElementById("filterTimeTo");

    const resetButton =
        document.getElementById("resetFilters");

    const resetEmptyButton =
        document.getElementById("resetFiltersEmpty");


    /* =====================================================
       SCHEDULE ELEMENTS
       ===================================================== */

    const lessonCards =
        Array.from(
            document.querySelectorAll(
                ".lesson-card[data-schedule='true']"
            )
        );


    /*
     * НОВАЯ СТРУКТУРА:
     *
     * Группа
     *   └── День
     *        └── Занятия
     */

    const groupCards =
        Array.from(
            document.querySelectorAll(".group-card")
        );

    const dayCards =
        Array.from(
            document.querySelectorAll(".group-day-card")
        );


    /* =====================================================
       COUNTERS / ANALYTICS
       ===================================================== */

    const totalLessonsElement =
        document.getElementById("totalLessons");

    const totalLessonsWordElement =
        document.getElementById("totalLessonsWord");

    const filteredLessonsCount =
        document.getElementById("filteredLessonsCount");

    const analyticsTotal =
        document.getElementById("analyticsTotal");

    const analyticsElement =
        document.getElementById("filterAnalytics");

    const noResults =
        document.getElementById("filterNoResults");


    /* =====================================================
       PRINT
       ===================================================== */

    printScheduleButton?.addEventListener(
        "click",
        function () {
            window.print();
        }
    );


    /* =====================================================
       WORD FORM
       ===================================================== */

    function getLessonWord(count) {

        if (
            count % 10 === 1 &&
            count % 100 !== 11
        ) {
            return "занятие";
        }

        if (
            count % 10 >= 2 &&
            count % 10 <= 4 &&
            (
                count % 100 < 10 ||
                count % 100 >= 20
            )
        ) {
            return "занятия";
        }

        return "занятий";
    }


    /* =====================================================
       TIME
       ===================================================== */

    function timeToMinutes(value) {

        if (!value)
            return null;

        const parts =
            value.split(":");

        if (parts.length < 2)
            return null;

        const hours =
            parseInt(parts[0], 10);

        const minutes =
            parseInt(parts[1], 10);

        if (
            Number.isNaN(hours) ||
            Number.isNaN(minutes)
        ) {
            return null;
        }

        return (
            hours * 60 +
            minutes
        );
    }


    /* =====================================================
       NORMALIZE SEARCH
       ===================================================== */

    function normalize(value) {

        return (value || "")
            .toLowerCase()
            .replace(/ё/g, "е")
            .trim();
    }


    /* =====================================================
       SEARCH
       ===================================================== */

    function matchesSearch(card) {

        const query =
            normalize(
                searchInput?.value
            );

        if (!query)
            return true;

        const text =
            normalize(
                card.dataset.search
            );

        return text.includes(query);
    }


    /* =====================================================
       TIME FILTER
       ===================================================== */

    function matchesTime(card) {

        const from =
            timeToMinutes(
                timeFromFilter?.value
            );

        const to =
            timeToMinutes(
                timeToFilter?.value
            );

        const start =
            timeToMinutes(
                card.dataset.start
            );

        const end =
            timeToMinutes(
                card.dataset.end
            );


        /*
         * Если у занятия нет времени,
         * не скрываем его.
         */

        if (
            start === null ||
            end === null
        ) {
            return true;
        }


        /*
         * Указаны оба значения.
         */

        if (
            from !== null &&
            to !== null
        ) {

            /*
             * Если диапазон введён неправильно,
             * ничего дополнительно не фильтруем.
             */

            if (to <= from)
                return true;


            /*
             * Проверяем пересечение интервалов.
             *
             * Например:
             *
             * фильтр 09:00 — 11:00
             * занятие 10:00 — 11:30
             *
             * оно будет найдено.
             */

            return (
                start < to &&
                end > from
            );
        }


        /*
         * Только "от".
         */

        if (from !== null) {
            return end > from;
        }


        /*
         * Только "до".
         */

        if (to !== null) {
            return start < to;
        }


        return true;
    }


    /* =====================================================
       CARD FILTER
       ===================================================== */

    function matchesCard(card) {

        /*
         * GROUP
         */

        if (
            groupFilter?.value &&
            card.dataset.groupId !==
            groupFilter.value
        ) {
            return false;
        }


        /*
         * TEACHER
         */

        if (
            teacherFilter?.value &&
            card.dataset.teacherId !==
            teacherFilter.value
        ) {
            return false;
        }


        /*
         * SUBJECT
         */

        if (
            subjectFilter?.value &&
            card.dataset.subjectId !==
            subjectFilter.value
        ) {
            return false;
        }


        /*
         * CLASSROOM
         */

        if (
            classroomFilter?.value &&
            card.dataset.classroomId !==
            classroomFilter.value
        ) {
            return false;
        }


        /*
         * DAY
         */

        if (
            dayFilter?.value &&
            card.dataset.day !==
            dayFilter.value
        ) {
            return false;
        }


        /*
         * SEARCH
         */

        if (!matchesSearch(card))
            return false;


        /*
         * TIME
         */

        if (!matchesTime(card))
            return false;


        return true;
    }


    /* =====================================================
       UPDATE DAY CARDS
       ===================================================== */

    function updateDayCards() {

        dayCards.forEach(dayCard => {

            const lessons =
                Array.from(
                    dayCard.querySelectorAll(
                        ".lesson-card[data-schedule='true']"
                    )
                );


            const visibleLessons =
                lessons.filter(
                    lesson =>
                        !lesson.classList.contains(
                            "is-filter-hidden"
                        )
                );


            const count =
                visibleLessons.length;


            /*
             * Показываем день только если
             * в нём осталось хотя бы одно занятие.
             */

            dayCard.style.display =
                count > 0
                    ? ""
                    : "none";


            /*
             * Счётчик дня.
             */

            const countElement =
                dayCard.querySelector(
                    ".js-day-count"
                );


            if (countElement) {

                countElement.textContent =
                    count;

            }


            /*
             * Склонение.
             */

            const wordElement =
                dayCard.querySelector(
                    ".js-day-word"
                );


            if (wordElement) {

                wordElement.textContent =
                    getLessonWord(count);

            }

        });
    }


    /* =====================================================
       UPDATE GROUP CARDS
       ===================================================== */

    function updateGroupCards() {

        groupCards.forEach(groupCard => {

            const visibleDays =
                Array.from(
                    groupCard.querySelectorAll(
                        ".group-day-card"
                    )
                ).filter(
                    dayCard =>
                        dayCard.style.display !== "none"
                );


            /*
             * Если после фильтрации
             * в группе ничего не осталось —
             * скрываем всю группу.
             */

            groupCard.style.display =
                visibleDays.length > 0
                    ? ""
                    : "none";


            /*
             * Обновляем общий счётчик
             * занятий внутри группы.
             */

            const visibleLessons =
                Array.from(
                    groupCard.querySelectorAll(
                        ".lesson-card[data-schedule='true']"
                    )
                ).filter(
                    lesson =>
                        !lesson.classList.contains(
                            "is-filter-hidden"
                        )
                );


            const groupCount =
                visibleLessons.length;


            /*
             * Второй элемент внутри заголовка
             * группы содержит:
             *
             * "5 занятий"
             */

            const groupSubtitle =
                groupCard.querySelector(
                    ".group-title span"
                );


            if (groupSubtitle) {

                groupSubtitle.textContent =
                    `${groupCount} ${getLessonWord(groupCount)}`;

            }

        });
    }


    /* =====================================================
       SELECT TEXT
       ===================================================== */

    function getSelectedText(select) {

        if (
            !select ||
            !select.value
        ) {
            return null;
        }


        const option =
            select.options[
                select.selectedIndex
            ];


        return option
            ? option.textContent.trim()
            : null;
    }


    /* =====================================================
       COUNT BY FILTER
       ===================================================== */

    function countLessonsBy(
        attribute,
        value
    ) {

        return lessonCards.filter(
            card =>
                card.dataset[attribute] === value &&
                !card.classList.contains(
                    "is-filter-hidden"
                )
        ).length;

    }


    /* =====================================================
       ANALYTICS
       ===================================================== */

    function updateAnalytics() {

        const visibleLessons =
            lessonCards.filter(
                card =>
                    !card.classList.contains(
                        "is-filter-hidden"
                    )
            );


        const visibleCount =
            visibleLessons.length;


        /*
         * Основной счётчик.
         */

        if (filteredLessonsCount) {

            filteredLessonsCount.textContent =
                visibleCount;

        }


        /*
         * Дополнительная аналитика,
         * если она существует в HTML.
         */

        if (analyticsTotal) {

            analyticsTotal.textContent =
                visibleCount;

        }


        /*
         * Если блока аналитики нет,
         * просто заканчиваем функцию.
         */

        if (!analyticsElement)
            return;


        const analyticsParts = [];


        /* =================================================
           GROUP
           ================================================= */

        if (groupFilter?.value) {

            const name =
                getSelectedText(
                    groupFilter
                );


            const count =
                countLessonsBy(
                    "groupId",
                    groupFilter.value
                );


            analyticsParts.push(
                `<span class="analytics-item">
                    Группа «${name}» —
                    <strong>${count}</strong>
                    ${getLessonWord(count)} в неделю
                </span>`
            );

        }


        /* =================================================
           TEACHER
           ================================================= */

        if (teacherFilter?.value) {

            const name =
                getSelectedText(
                    teacherFilter
                );


            const count =
                countLessonsBy(
                    "teacherId",
                    teacherFilter.value
                );


            analyticsParts.push(
                `<span class="analytics-item">
                    ${name} —
                    <strong>${count}</strong>
                    ${getLessonWord(count)} в неделю
                </span>`
            );

        }


        /* =================================================
           SUBJECT
           ================================================= */

        if (subjectFilter?.value) {

            const name =
                getSelectedText(
                    subjectFilter
                );


            const count =
                countLessonsBy(
                    "subjectId",
                    subjectFilter.value
                );


            analyticsParts.push(
                `<span class="analytics-item">
                    «${name}» —
                    <strong>${count}</strong>
                    ${getLessonWord(count)} в неделю
                </span>`
            );

        }


        /* =================================================
           CLASSROOM
           ================================================= */

        if (classroomFilter?.value) {

            const name =
                getSelectedText(
                    classroomFilter
                );


            const count =
                countLessonsBy(
                    "classroomId",
                    classroomFilter.value
                );


            analyticsParts.push(
                `<span class="analytics-item">
                    Аудитория «${name}» —
                    <strong>${count}</strong>
                    ${getLessonWord(count)} в неделю
                </span>`
            );

        }


        /* =================================================
           OUTPUT
           ================================================= */

        if (analyticsParts.length > 0) {

            analyticsElement.innerHTML =
                analyticsParts.join(
                    '<span class="analytics-muted"> · </span>'
                );

        }
        else {

            analyticsElement.innerHTML = `
                <span class="analytics-muted">
                    Общая нагрузка:
                </span>

                <strong>
                    ${visibleCount}
                </strong>

                <span class="analytics-muted">
                    ${getLessonWord(visibleCount)}
                    в неделю
                </span>
            `;

        }

    }


    /* =====================================================
       UPDATE TOTAL
       ===================================================== */

    function updateTotal() {

        const visibleCount =
            lessonCards.filter(
                card =>
                    !card.classList.contains(
                        "is-filter-hidden"
                    )
            ).length;


        if (totalLessonsElement) {

            totalLessonsElement.textContent =
                visibleCount;

        }


        if (totalLessonsWordElement) {

            totalLessonsWordElement.textContent =
                getLessonWord(
                    visibleCount
                );

        }

    }


    /* =====================================================
       APPLY FILTERS
       ===================================================== */

    function applyFilters() {

        let visibleCount = 0;


        /*
         * Фильтруем каждое занятие.
         */

        lessonCards.forEach(card => {

            const match =
                matchesCard(card);


            card.classList.toggle(
                "is-filter-hidden",
                !match
            );


            /*
             * Не используем display:none
             * непосредственно на карточке.
             *
             * Это позволяет CSS нормально
             * работать с новой структурой.
             */

            if (match) {

                card.style.display = "";

                visibleCount++;

            }
            else {

                card.style.display = "none";

            }

        });


        /*
         * Обновляем дни.
         */

        updateDayCards();


        /*
         * Обновляем группы.
         */

        updateGroupCards();


        /*
         * Обновляем общий счётчик.
         */

        updateTotal();


        /*
         * Обновляем аналитику.

         */

        updateAnalytics();


        /*
         * Сообщение "ничего не найдено".
         */

        if (noResults) {

            noResults.style.display =
                visibleCount === 0
                    ? "flex"
                    : "none";

        }

    }


    /* =====================================================
       RESET FILTERS
       ===================================================== */

    function resetFilters() {

        if (searchInput)
            searchInput.value = "";


        if (groupFilter)
            groupFilter.value = "";


        if (teacherFilter)
            teacherFilter.value = "";


        if (subjectFilter)
            subjectFilter.value = "";


        if (classroomFilter)
            classroomFilter.value = "";


        if (dayFilter)
            dayFilter.value = "";


        if (timeFromFilter)
            timeFromFilter.value = "";


        if (timeToFilter)
            timeToFilter.value = "";


        /*
         * Повторно применяем фильтры.
         */

        applyFilters();

    }


    /* =====================================================
       FILTER EVENTS
       ===================================================== */

    [
        searchInput,
        groupFilter,
        teacherFilter,
        subjectFilter,
        classroomFilter,
        dayFilter,
        timeFromFilter,
        timeToFilter
    ].forEach(element => {

        if (!element)
            return;


        /*
         * Для поиска.

         */

        element.addEventListener(
            "input",
            applyFilters
        );


        /*
         * Для select и time input.
         */

        element.addEventListener(
            "change",
            applyFilters
        );

    });


    /* =====================================================
       RESET BUTTONS
       ===================================================== */

    resetButton?.addEventListener(
        "click",
        resetFilters
    );


    resetEmptyButton?.addEventListener(
        "click",
        resetFilters
    );


    /* =====================================================
       INITIAL FILTER STATE
       ===================================================== */

    applyFilters();

});

