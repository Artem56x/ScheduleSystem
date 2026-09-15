    function submitExport(type, format) {

    let select;
    let action;

    if (type === "group") {

    select =
    document.getElementById("groupSelect");

    action =
    "ExportGroup";

    }
    else if (type === "teacher") {

    select =
    document.getElementById("teacherSelect");

    action =
    "ExportTeacher";
    }

    if (!select || !select.value) {

    select?.focus();

    return;
    }

    const id = select.value;

    const formattedFormat =
    format.charAt(0).toUpperCase() +
    format.slice(1);

    window.location.href =
    `/Schedules/${action}${formattedFormat}?id=${encodeURIComponent(id)}`;
    }