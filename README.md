# ProgressionEcole

A simple web application for generating student achievement reports.
This application has been totally vibe coded with copilot

## Project Overview
ProgressionEcole is designed for kindergarten students, following the Montessori pedagogy. It helps teachers track and report student achievements throughout the school year. The app allows you to:
- Configure your student database
- Define and manage learning achievements ("acquis")
- Associate achievements with students for each of the 5 periods in the year
- Generate a document listing all achievements for all students, period by period

## Technologies Used
- **Frontend:** Angular (TypeScript)
- **Backend:** ASP.NET Core (.NET 9)
- **Data Storage:** JSON files (no database)

## Structure
- `progression-ecole-front/` — Angular web application
- `progression-ecole-back/` — ASP.NET Core API
- All data (students, achievements, periods, categories) is stored in JSON files in the `progression-ecole-back/Data/` folder

## What the Frontend Does
- Displays students, achievements, and periods
- Allows you to select and associate achievements for each student and period
- Lets you generate and download a Word document summarizing all achievements
- Communicates with the backend API to read/write data

## What the Backend Does
- Serves the API for managing students, achievements, periods, and categories
- Handles document generation (Word format)
- Reads and writes all data to JSON files (no database required)

## How to Build and Run

### Backend (API)
1. Go to the `progression-ecole-back` folder
2. Build and run:
   ```sh
   dotnet build
   dotnet run
   ```
   Or publish for deployment:
   ```sh
   dotnet publish -c Release -o ./publish
   ```

### Frontend (Angular)
1. Go to the `progression-ecole-front` folder
2. Install dependencies:
   ```sh
   npm install
   ```
3. Build and run:
   ```sh
   ng serve
   ```
   Or build for production:
   ```sh
   ng build --configuration production
   ```

## Deployment
- You can deploy both the API and the Angular app to Azure Web Apps or any other hosting platform.
- Make sure the frontend is configured to call the correct API URL.
- All data is stored in files, so ensure the backend has write access to its `Data/` folder.

## Notes
- No database is required; all data is file-based.
- The document generation uses Word templates and outputs `.docx` files.
- The app is designed for simplicity and ease of use for teachers.

---
Feel free to contribute or adapt for your own needs, but the first purpose is for my own usage.
