BICTECH 🖥️📱

BICTECH is an e-commerce web application where users can browse and buy electronic devices such as smartphones, smartwatches, chargers, speakers, and more.

This project was developed as a final assignment for an academic course and demonstrates a complete full-stack implementation using React (frontend) and .NET 8 (backend).

The system includes authentication with role management, pagination, logging, testing, and automatic API documentation.

🧩 GENERAL OVERVIEW

🏷️ Project Name: BICTECH

🛒 Type: E-commerce / Online Store

⚙️ Architecture: Clean Architecture (Domain, Application, Infrastructure, Presentation)

💾 Database: SQL Server

🧠 Tests: Unit Tests & Integration Tests

🔄 CI/CD: Automated with GitHub Actions

🧑‍💻 Developed with: React + .NET 8 (ASP.NET Core)

🌐 Automatic Swagger URL: http://localhost:5000

💻 FRONTEND (React)

The frontend is located in the BicTechFront folder.
It provides a simple and clean UI for browsing, searching, and purchasing products.

Main features:

⚛️ Built with React

🌍 Uses the Fetch API (no Axios) for HTTP requests

📱 Displays product lists with pagination

🔗 Connects to backend via VITE_API_URL (from .env file)

🎨 Responsive and user-friendly layout

To run the frontend:

Open a terminal in the BicTechFront folder

Run:
npm install
npm run dev

Open the browser at the URL shown (usually http://localhost:5173
)

🧱 BACKEND (.NET 8 + Clean Architecture)

The backend is located in the BicTechBack folder.
It follows the Clean Architecture pattern, separating code into layers for better maintainability and scalability.

Structure:

🧩 Domain: Core entities and business rules

🧠 Application: Services, DTOs, and use cases

🗄️ Infrastructure: Data access and EF Core configuration

🌐 Presentation: Controllers and API endpoints

Main features:

🧾 Entity Framework Core for data persistence

🔐 Role-based authentication (Admin / Client)

🧰 Swagger documentation (auto-opens on startup)

🪵 Logging for key operations

🔄 Pagination and generic repository pattern

🧪 Unit and integration tests with xUnit

🧱 SQL Server database

To run the backend:

Open a terminal in the BicTechBack folder

Run:
dotnet restore
dotnet run

Once it starts, the Swagger UI will open automatically at:
👉 http://localhost:5000

🧰 DATABASE

💾 The project uses SQL Server as the database engine.

🧩 The SQL script to create the database is located inside the DataBase folder.

⚙️ Update your connection string inside appsettings.json before running the backend.

Example:
"ConnectionStrings": {
"DefaultConnection": "Server=YOUR_SERVER;Database=BicTechDB;Trusted_Connection=True;"
}

🚀 CI/CD PIPELINE

The project includes a Continuous Integration workflow using GitHub Actions.
Each time a commit or pull request is made to the main branch:

The backend dependencies are restored

The project is built in Release mode

All tests (unit + integration) are executed

This ensures that the project remains stable, tested, and ready for deployment.

🧭 HOW TO CLONE OR DOWNLOAD

You can get the project in two simple ways:

Option 1 (recommended):

Open your terminal

Run:
git clone https://github.com/emi-pereyra17/TP-Final-BICTECH.git

Option 2:

Click the green “Code” → “Download ZIP” button on GitHub

Unzip the folder

Open it in your code editor (like Visual Studio or VS Code)

⚙️ QUICK SETUP SUMMARY

Clone or download the repo

Create the database using the provided script

Update appsettings.json with your SQL Server credentials

Run the backend (dotnet run) → Swagger opens at http://localhost:5000

Run the frontend (npm run dev)

Make sure the VITE_API_URL in .env matches your backend port (default: 5000)

Enjoy exploring the BICTECH store! 🎉

👥 TEAM

Developed by:
Máximo Jara
Emiliano Pereyra
Santiago Pérez