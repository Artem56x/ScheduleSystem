/* ========================================================= */
/* ПРЕПОДАВАТЕЛЬ */
/* ========================================================= */

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


/* ========================================================= */
/* РЕКОМЕНДАЦИИ ПРЕПОДАВАТЕЛЕЙ */
/* ========================================================= */

function selectTeacher(teacherId) {

    const select =
        document.getElementById(
            "TeacherId"
        );

    if (!select) {

        return;

    }


    select.value =
        teacherId;


    select.dispatchEvent(
        new Event("change", {
            bubbles: true
        })
    );


    select.focus();


    closeTeacherRecommendations();

}


function closeTeacherRecommendations() {

    const modal =
        document.getElementById(
            "teacherRecommendationModal"
        );

    if (!modal) {

        return;

    }


    modal.remove();

}


function closeTeacherConflict() {

    const modal =
        document.getElementById(
            "teacherConflictModal"
        );

    if (!modal) {

        return;

    }


    modal.remove();

}


/* ========================================================= */
/* АУДИТОРИИ */
/* ========================================================= */

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

    if (!modal) {

        return;

    }


    modal.remove();

}


function closeNoClassroomRecommendations() {

    const modal =
        document.getElementById(
            "classroomNoRecommendationModal"
        );

    if (!modal) {

        return;

    }


    modal.remove();

}


/* ========================================================= */
/* ОБЩАЯ ОШИБКА ВАЛИДАЦИИ */
/* ========================================================= */

function closeValidationError() {

    const modal =
        document.getElementById(
            "validationErrorModal"
        );

    if (!modal) {

        return;

    }


    modal.remove();

}

