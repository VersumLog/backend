# VersomLog-backend

# ВСТАНОВЛЕННЯ
1. Встанови Visual Studio 2026 та .NET 10 SDK.

2. У Visual Studio Installer обери вкладку "ASP.NET and web development" (перша галочка).

3. Створи в папці проекту файл appsettings.Development.json (якщо його немає).

4. Скопіюй туди Connection String від актуальної бази даних (запитай у саші або візьми з закріплених повідомлень).

5. Відкрий проект (.sln) у Visual Studio. Вона автоматично почне скачувати пакети (Restore NuGet Packages).

Якщо ти щось написав в коді, оновити базу даних можна так:
 Відкрий Package Manager Console (Tools -> NuGet Package Manager) та введи:
    Add-Migration "Назва Зміни"
    Update-Database

Для запуску:
Натисни F5 (зелена стрілочка зверху) у Visual Studio.
При першому запуску будуть просити створити локальний Ssl ключ, просто зі всім погоджуйся

Результат зазвичай за посиланнями:
https://localhost:7014/swagger/
http://localhost:5056/swagger/