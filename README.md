# WebBlog

WebBlog — это простое backend-приложение для микроблогов на .NET 8. Предоставляет веб-API для управления постами, пользователями и загрузки изображений. (Special for CPT)

## Сценарии запуска

### 1. Ручная компиляция и запуск (с локальной PostgreSQL или файловой БД)

#### Требования:
- .NET 8 SDK
- PostgreSQL 15+
- Средство сборки (Visual Studio / .NET CLI)

#### Настройка базы данных (Postgres по-умолчанию):
**Если вы не планируете использовать Postgres БД, пропустите этот шаг.**
1. Установите PostgreSQL и создайте базу данных:
   ```sql
   CREATE DATABASE webblog;
   CREATE USER webblog_user WITH PASSWORD 'your_password';
   GRANT ALL PRIVILEGES ON DATABASE webblog TO webblog_user;
   ```

2. Настройте подключение в `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=webblog;Username=webblog_user;Password=your_password"
   }
   ```

#### Сборка:
```bash
dotnet restore
dotnet build
```

#### Запуск:
**Если вы используете Postgres БД:**
```bash
dotnet run --project WebBlog
```
**Если вы используете Файловую БД (не требует дополнительного ПО):**
```bash
dotnet run --project WebBlog -- --database=file
```

Приложение будет доступно на порту 80:  
http://localhost:80

### 2. Запуск через Docker

#### Требования:
- Docker 20.10+
- Docker Compose

#### Запуск с Docker Compose:
1. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/tdlcompiler/WebBlog.git
   cd webblog
   ```

2. Запустите сервисы из корневой директории проекта:
   ```bash
   docker-compose up
   ```
Приложение будет доступно на порту 5000:  
http://localhost:5000

#### Все Docker-команды должны выполняться из корневой директории проекта, где находится `docker-compose.yml`
- Для остановки и очистки используйте:

  ```bash
  docker-compose down -v
  ```

## Документация API

После запуска доступна Swagger-документация по адресу:  
- http://localhost:80/swagger (после ручного запуска)
- http://localhost:5000/swagger (после запуска в Docker)

## Конфигурация
Пример основных параметров в `appsettings.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:80"
      }
    }
  },
  "Jwt": {
    "Secret": "your_jwt_secret_key"
  }
}
```

## Структура проекта
- **Controllers**: API endpoints
- **Data**: Local Database (Файловая БД, включает в себя users.json и posts.json) - используется, если включен FileRepository
- **Data/images**: Хранилище изображений
- **Models**: Сущности БД (User, Post, Image)
- **Services**: Бизнес-логика
- **Migrations**: Миграции базы данных

## Миграции базы данных
Для ручного применения миграций:
```bash
dotnet ef database update
```

## Переменные окружения для Docker
|Переменная| Описание|
|-|-|
|ConnectionStrings__DefaultConnection|Строка подключения к PostgreSQL|
| Jwt__Secret|Секрет для генерации JWT|
