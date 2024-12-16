# WebBlog

WebBlog — это простое backend-приложение для микроблогов, реализованное на .NET 8. Оно предоставляет веб-API для работы с постами и пользователями, а также для загрузки изображений в посты. (Special for CPT)

## Старт проекта

### Шаг 1: Сборка проекта

Для начала необходимо скомпилировать проект. Для этого откройте файл решения `WebBlog.sln` в Visual Studio и соберите проект.

### Шаг 2: Публикация и запуск

После успешной компиляции и публикации вы можете запустить сервер, запустив файл `WebBlog.exe`.

### Шаг 3: Запуск веб-сервера

После запуска сервер веб-API будет доступен по адресу: http://localhost:5225

## Документация

Для получения документации по доступным методам API и их описаниям, перейдите по следующему адресу: http://localhost:5225/swagger

Документация отображает все доступные эндпоинты и их параметры.

## Структура данных

Проект хранит данные локально в папке **data**.

- **users.json**: файл, в котором содержатся данные о пользователях.
- **posts.json**: файл, в котором содержатся данные о постах.
- **images**: папка, в которой сохраняются все изображения, загруженные в посты.

## Структура проекта

- **Controllers**: контроллеры для обработки запросов.
- **Models**: модели данных для пользователей, постов и запросов.
- **Services**: сервисы для бизнес-логики.
- **data/images**: папка для хранения изображений.
- **data**: папка для хранения данных в формате JSON (users.json и posts.json).

## Требования

- .NET 8 SDK
- Visual Studio для компиляции проекта
