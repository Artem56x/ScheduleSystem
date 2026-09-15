    document.addEventListener("DOMContentLoaded", function () {

const printScheduleButton =
    document.getElementById("printScheduleButton");

printScheduleButton?.addEventListener(
    "click",
    function () {
        window.print();
    }
);

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

        const lessonCards =
            Array.from(
                document.querySelectorAll(
                    ".lesson-card[data-schedule='true']"
                )
            );

        const dayCards =
            Array.from(
                document.querySelectorAll(".day-card")
            );

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

            const parts = value.split(":");

            if (parts.length < 2)
                return null;

            return (
                parseInt(parts[0], 10) * 60 +
                parseInt(parts[1], 10)
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
                normalize(searchInput?.value);

            if (!query)
                return true;

            const text =
                normalize(card.dataset.search);

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

            if (
                start === null ||
                end === null
            ) {
                return true;
            }


            if (
                from !== null &&
                to !== null
            ) {

                if (to <= from)
                    return true;

                return (
                    start < to &&
                    end > from
                );
            }


            if (from !== null)
                return end > from;


            if (to !== null)
                return start < to;


            return true;
        }


        /* =====================================================
           CARD FILTER
           ===================================================== */

        function matchesCard(card) {

            if (
                groupFilter.value &&
                card.dataset.groupId !==
                groupFilter.value
            ) {
                return false;
            }


            if (
                teacherFilter.value &&
                card.dataset.teacherId !==
                teacherFilter.value
            ) {
                return false;
            }


            if (
                subjectFilter.value &&
                card.dataset.subjectId !==
                subjectFilter.value
            ) {
                return false;
            }


            if (
                classroomFilter.value &&
                card.dataset.classroomId !==
                classroomFilter.value
            ) {
                return false;
            }


            if (
                dayFilter.value &&
                card.dataset.day !==
                dayFilter.value
            ) {
                return false;
            }


            if (!matchesSearch(card))
                return false;


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


                dayCard.style.display =
                    count > 0
                        ? ""
                        : "none";


                const countElement =
                    dayCard.querySelector(
                        ".js-day-count"
                    );

                const wordElement =
                    dayCard.querySelector(
                        ".js-day-word"
                    );


                if (countElement) {

                    countElement.textContent =
                        count;

                }


                if (wordElement) {

                    wordElement.textContent =
                        getLessonWord(count);

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
                    card.dataset[attribute] === value
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


            filteredLessonsCount.textContent =
                visibleCount;


            analyticsTotal.textContent =
                visibleCount;


            const analyticsParts = [];


            /* GROUP */

            if (groupFilter.value) {

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


            /* TEACHER */

            if (teacherFilter.value) {

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


            /* SUBJECT */

            if (subjectFilter.value) {

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


            /* CLASSROOM */

            if (classroomFilter.value) {

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
                        ${getLessonWord(visibleCount)} в неделю
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


            totalLessonsElement.textContent =
                visibleCount;


            totalLessonsWordElement.textContent =
                getLessonWord(
                    visibleCount
                );

        }


        /* =====================================================
           APPLY FILTERS
           ===================================================== */

        function applyFilters() {

            let visibleCount = 0;


            lessonCards.forEach(card => {

                const match =
                    matchesCard(card);


                card.classList.toggle(
                    "is-filter-hidden",
                    !match
                );


                if (match)
                    visibleCount++;

            });


            updateDayCards();

            updateTotal();

            updateAnalytics();


            if (noResults) {

                noResults.style.display =
                    visibleCount === 0
                        ? "flex"
                        : "none";

            }

        }


        /* =====================================================
           RESET
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


            applyFilters();

        }


        /* =====================================================
           EVENTS
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


            element.addEventListener(
                "input",
                applyFilters
            );


            element.addEventListener(
                "change",
                applyFilters
            );

        });


        resetButton?.addEventListener(
            "click",
            resetFilters
        );


        resetEmptyButton?.addEventListener(
            "click",
            resetFilters
        );

printScheduleButton?.addEventListener(
    "click",
    function () {
        window.print();
    }
);
        /* =====================================================
           INITIAL
           ===================================================== */

        applyFilters();
       }) 
