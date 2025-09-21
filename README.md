# Microservices

## Таск трекер

### Функциональные требования:
1. Пользователи могут регистрироваться и логиниться.
2. Пользователь может создавать/редактировать/удалять проекты.
3. В проекте можно создавать задачи (Task), назначать исполнителя, ставить статус и дедлайн.
4. Задачи можно комментировать и прикреплять теги.
5. По проектам и задачам доступен поиск и фильтрация (по статусу, исполнителю, тегам, дате).
6. Роли: админ (управляет пользователями и проектами) и пользователь (работает с задачами своих проектов).
7. История изменения статусов задач (лог действий) для аудита.

### Сервисы:
1. AuthService — регистрация, вход, обновление пароля, выдача JWT.
2. UserService — CRUD пользователей, роли.
3. ProjectService — CRUD проектов, привязка пользователей к проекту.
4. TaskService — CRUD задач, назначение, изменение статусов, комментарии, теги, лог статусов.
5. SearchService — агрегация/фильтрация по проектам/задачам.

System Design [схема](https://miro.com/welcomeonboard/MlZYeXBnbk1sbDlXMEg0M1ZtOGxIUVMrWklqOFZwK1l4SVhTNUdEUXJ0djBUV2llTnV2SG5uam1SNk1INFhnUzFOb25iZFk4anZ4cmhMWFI2ZVBKUU9nZ3N6TjBuQ3RWVTQwSVdHNld3cU91dG8vdEY4bWYrNDIxTFVDOGpnRGlBS2NFMDFkcUNFSnM0d3FEN050ekl3PT0hdjE=?share_link_id=978292556443)

### Сущности TaskService:
* Task – задача внутри проекта
* TaskComment – комментарий к задаче
* TaskTag – тег задачи (many-to-many)
* Tag – справочник тегов
* TaskStatusLog – история изменения статусов задачи

### REST:

#### TaskService

(Создать задачу)
POST /api/v1/tasks (title, description, projectId, assigneeId?, dueDate?, priority)

(Получить задачу)
GET /api/v1/tasks/{taskId}

(Список задач)
GET /api/v1/tasks?projectId=&assigneeId=&status=&tag=&page=&pageSize=

(Обновить задачу)
PATCH /api/v1/tasks/{taskId} (title?, description?, assigneeId?, dueDate?, priority?)

(Изменить статус)
POST /api/v1/tasks/{taskId}/status (newStatus)

(Удалить задачу)
DELETE /api/v1/tasks/{taskId}

#### AuthService

(Регистрация по email)
POST /api/v1/auth/register/email (firstName, lastName, email, password)

(Логин по email)
POST /api/v1/auth/login/email (email, password)

(Обновление токена)
POST /api/v1/auth/refresh (refreshToken)

(Выход)
POST /api/v1/auth/logout (refreshToken)

#### UserService

(Создать пользователя)
POST /api/v1/users (firstName, lastName, email, role)

(Получить пользователя)
GET /api/v1/users/{userId}

(Список пользователей)
GET /api/v1/users?page=&pageSize=&role=

(Обновить профиль пользователя)
PATCH /api/v1/users/{userId} (firstName?, lastName?, email?, role?)

(Удалить пользователя)
DELETE /api/v1/users/{userId}

#### ProjectService

(Создать проект)
POST /api/v1/projects (name, description?, ownerId)

(Получить проект)
GET /api/v1/projects/{projectId}

(Список проектов)
GET /api/v1/projects?ownerId=&page=&pageSize=

(Обновить проект)
PATCH /api/v1/projects/{projectId} (name?, description?, isArchived?)

(Удалить проект)
DELETE /api/v1/projects/{projectId}

#### SearchService

(Поиск задач и проектов)
GET /api/v1/search?q=&type=&page=&pageSize=
Параметры:

q — строка поиска

type — task | project | user

page, pageSize — пагинация

Результат: массив объектов (tasks[], projects[], users[]) с базовыми полями