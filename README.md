# Ntuzuma Clinic Booking System

A full-stack clinic booking system designed for university health clinics. Patients can book appointments, manage their bookings, and communicate with the clinic, while administrators have full control over appointments, users, and messages.

--

🔗 Live Demo: :[Booking App](https://dut-clinic-booking-systemfrontend.onrender.com)

--

🛠 Tech Stack
Backend: C#, .NET Core 10, Entity Framework Core

Frontend: React, React Router, Vite

Database: PostgreSQL

Authentication: JWT, BCrypt

Email: MailKit (SMTP)

Other Tools: dotenv, AspNetCoreRateLimit

--

# Authentication & Security
User Registration with email verification

Secure Login with JWT authentication

Password Reset flow with 6-digit verification codes

Role-based Access Control (Admin vs User)

Rate Limiting to prevent brute force attacks

BCrypt Password Hashing for secure storage

JWT Token Expiry (1 hour default, customizable via Remember Me)

--

# 📸 Screenshots

![home page](/Screenshot(447).png)

![home page](/Screenshot(448).png)

--

# How to Run the App

git clone https://github.com/StheloNgcoborh01/Dut-Clinic-Booking-System.git
cd Dut-Clinic-Booking-System

--
#Backend Setup:

cd Backend/Backend
dotnet restore
dotnet ef database update
dotnet run

--
# Frontend Setup:
cd FrontEnd
npm install
npm run dev

--

#Author
Asanda Ngcobo

GitHub: @StheloNgcoborh01

LinkedIn: Asanda Ngcobo

