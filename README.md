# API CRUD with Postman and SQL Server

This project consists of an API developed in Postman to support HTTP requests, performing CRUD (Create, Read, Update, Delete) operations on a SQL Server database. The database and API processes are stored within the `SQLePostman` folder, located in the `.vs` directory.

## 📌 Technologies Used
- **Postman**: Tool used to create and test the API.
- **SQL Server**: Database used to store information.
- **.NET (if applicable)**: In case it was used to mediate requests.

## 🔧 Features
The API allows performing the following operations:
- **Create**: Add new records to the database.
- **Read**: Retrieve existing records.
- **Update**: Modify existing records.
- **Delete**: Remove records from the database.

## 📂 Project Structure
```
.vs/
 ├── SQLePostman/
 │   ├── Database.sql        # Database script
 │   ├── Requests.postman.json   # Postman request collection
 │   ├── API_Documentation.md    # Detailed API documentation
```

## 🚀 How to Use

### 1️⃣ Clone the Repository
```bash
git clone <REPOSITORY_URL>
cd <PROJECT_NAME>
```

### 2️⃣ Import the Database
1. Open SQL Server Management Studio (SSMS) or another compatible tool.
2. Run the `Database.sql` script to create tables and populate data.

### 3️⃣ Import Requests into Postman
1. Open Postman.
2. Click on **File > Import** and select the `Requests.postman.json` file.
3. Execute the requests as needed.

## 📌 Author
Developed by Henrico Birochi.

