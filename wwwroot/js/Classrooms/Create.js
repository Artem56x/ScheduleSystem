const modal = document.getElementById("categoryModal");

const openModalButton =
    document.getElementById("openCategoryModal");

const closeModalButton =
    document.getElementById("closeCategoryModal");

const cancelButton =
    document.getElementById("cancelCategory");

const saveButton =
    document.getElementById("saveCategory");

const categoryInput =
    document.getElementById("newCategoryName");

const categorySelect =
    document.getElementById("classroomCategorySelect");

const categoryError =
    document.getElementById("categoryError");

const antiForgeryToken =
    document.querySelector(
        'input[name="__RequestVerificationToken"]'
    ).value;

const createCategoryUrl =
    modal.dataset.createCategoryUrl;


// Открытие окна

function openCategoryModal() {

    modal.classList.add("active");

    categoryInput.value = "";

    categoryError.textContent = "";

    categoryError.classList.remove("visible");

    setTimeout(function () {

        categoryInput.focus();

    }, 100);
}


// Закрытие окна

function closeCategoryModal() {

    modal.classList.remove("active");

    categoryInput.value = "";

    categoryError.textContent = "";

    categoryError.classList.remove("visible");
}


// Кнопка открытия

openModalButton.addEventListener(
    "click",
    openCategoryModal
);


// Кнопка закрытия

closeModalButton.addEventListener(
    "click",
    closeCategoryModal
);


// Отмена

cancelButton.addEventListener(
    "click",
    closeCategoryModal
);


// Закрытие по клику на фон

modal.addEventListener(
    "click",
    function (event) {

        if (event.target === modal) {

            closeCategoryModal();

        }

    }
);


// Закрытие по Escape

document.addEventListener(
    "keydown",
    function (event) {

        if (
            event.key === "Escape" &&
            modal.classList.contains("active")
        ) {

            closeCategoryModal();

        }

    }
);


// Добавление категории

saveButton.addEventListener(
    "click",
    async function () {

        const name =
            categoryInput.value.trim();


        if (!name) {

            categoryError.textContent =
                "Введите название категории.";

            categoryError.classList.add(
                "visible"
            );

            categoryInput.focus();

            return;
        }


        saveButton.disabled = true;

        saveButton.textContent =
            "Добавление...";


        try {

            const response = await fetch(
                createCategoryUrl,
                {
                    method: "POST",

                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken":
                            antiForgeryToken
                    },

                    body: JSON.stringify({
                        name: name
                    })
                }
            );


            const data =
                await response.json();


            if (
                !response.ok ||
                !data.success
            ) {

                categoryError.textContent =
                    data.message ||
                    "Не удалось добавить категорию.";

                categoryError.classList.add(
                    "visible"
                );

                return;
            }


            // Создаём новый пункт списка

            const option =
                document.createElement("option");

            option.value = data.id;

            option.textContent = data.name;


            categorySelect.appendChild(
                option
            );


            // Автоматически выбираем новую категорию

            categorySelect.value =
                data.id;


            closeCategoryModal();

        }
        catch (error) {

            console.error(
                "Ошибка добавления категории:",
                error
            );

            categoryError.textContent =
                "Произошла ошибка. Попробуйте ещё раз.";

            categoryError.classList.add(
                "visible"
            );

        }
        finally {

            saveButton.disabled = false;

            saveButton.textContent =
                "Добавить категорию";

        }

    }
);


// Добавление категории по Enter

categoryInput.addEventListener(
    "keydown",
    function (event) {

        if (event.key === "Enter") {

            event.preventDefault();

            saveButton.click();

        }

    }
);

