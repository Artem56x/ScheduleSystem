    /* ================================================= */
    /* ПРЕПОДАВАТЕЛЬ */
    /* ================================================= */

    function confirmTeacher() {

        const confirmInput =
            document.getElementById(
                "confirmTeacherSubject"
            );

        const form =
            document.getElementById(
                "scheduleForm"
            );

        if (confirmInput) {

            confirmInput.value = "true";

        }

        if (form) {

            form.submit();

        }

    }


    function closeTeacherWarning() {

        const warning =
            document.getElementById(
                "teacherWarning"
            );

        if (warning) {

            warning.remove();

        }

    }


    /* ================================================= */
    /* АУДИТОРИИ */
    /* ================================================= */

    function selectClassroom(classroomId) {

        const classroomSelect =
            document.getElementById(
                "ClassroomId"
            );

        if (!classroomSelect) {

            return;

        }


        classroomSelect.value =
            classroomId;


        classroomSelect.dispatchEvent(
            new Event("change", {
                bubbles: true
            })
        );


        classroomSelect.focus();


        closeClassroomRecommendations();

    }


    function closeClassroomRecommendations() {

        const modal =
            document.getElementById(
                "classroomRecommendationModal"
            );

        if (modal) {

            modal.remove();

        }

    }


    function closeNoClassroomRecommendations() {

        const modal =
            document.getElementById(
                "classroomNoRecommendationModal"
            );

        if (modal) {

            modal.remove();

        }

    }
