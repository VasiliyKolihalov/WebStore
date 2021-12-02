# WebStore
## Что такое WebStore?
 Это учебный проект, c взаимодействием WEB API REST написанный на C# с использованием ASP.NET Core

--- 
## Какую задачу он выполняет?
 WebStore выполняет функцию интернет магазина

---

## Какие в нём использованы технологии?
* Entity Framework
* MS Identity
* Automapper
* Scriban
---
## Какие фичи есть у WebStore?
* Аутентификация и авторизация
* Взаимодейтсвие с карточками продуктов
* Взаимодействие с корзиной
* Управления пользователями и ролями
* Взаимодействие с картинками
* Управление иерархией категорий товаров  

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
