# WebStore
## Что такое WebStore?
 Это учебный Restful проект написанный на C# с использованием ASP.NET Core

--- 
## Какую задачу он выполняет?
 WebStore выполняет функцию маркетплейса

---

## Какие в нём использованы технологии?
* Entity Framework
* MS Identity
* Automapper
* Scriban
* Moq
* Xunit
* MailKit
---
## Какие фичи есть у WebStore?
* Трёхуровневая архитектура 
* Аутентификация и авторизация
* Взаимодейтсвие с карточками продуктов
* Взаимодействие с корзиной
* Управления пользователями и ролями
* Взаимодействие с картинками
* Управление иерархией категорий товаров  
* Система продавцов
* Теги
* Юнит тесты
* 2FA
* Пагинация
* Взаимодействие с рейтингом продуктов
* Кеширование
* Отображение валют на основе ответа ЦБР

---
## Как запустить проект?
 Перейдите в папку WebStoreAPI по адресу "расположение проекта/WebStoreAPI/WebStoreAPI". Cоздайте два файла "adminsettings.json" и  "companysettings.json".
 Заполните их по следующим шаблонам c учётом валидации пароля:
 
 * Для adminsettings.json:
``` json
 {  
     "AdminData": {      
     "Name": "xxxx",
     "Email": "xxxxxx@xxxx.xxx",
     "Password": "xxxxxxx"
     }
}
 ```

 * Для companysettings.json:
 ``` json
{
    "CompanyData": {
    "Name": "xxxxx",
    "Email": "xxxxx@xxxxx.xxx",
    "EmailPassword": "xxxxxxxxx"
    }
}

```
