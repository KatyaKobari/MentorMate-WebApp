# MentorMate

MentorMate is a comprehensive web-based mentorship platform built with ASP.NET MVC that connects **mentors** and **mentees** through structured profiles, mentorship requests, real-time chat, and a collaborative community space. The platform provides role-based dashboards and a streamlined experience for building professional guidance relationships.
<img width="1763" height="715" alt="image" src="https://github.com/user-attachments/assets/126faa50-b435-4f6a-aaad-2c77fb3bf81f" />

---

## 🚀 Project Overview
- **Goal**: Bridge the gap between learners and experts by enabling accessible, structured, and ongoing mentorship relationships.  
- **Users**: Students, early-career professionals (mentees), and experienced professionals (mentors).  
- **Core Features**: User authentication, profile management, role-based dashboards, mentorship requests, real-time chat, community-driven Mentor Space, and notification system.  

---

## 🛠️ Tech Stack
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap 5.3.2
- **Backend**: ASP.NET Core MVC 8.0
- **Database**: SQL Server with Entity Framework Core 9.0
- **Authentication**: Custom session-based authentication with password hashing
- **UI Libraries**: Bootstrap Icons, AOS (Animate On Scroll)
- **Development Tools**: Entity Framework Migrations, Visual Studio

---

## 📑 Website Pages & Features

### 🌐 Landing Page
Modern, responsive homepage featuring:
- Hero section with call-to-action
- Feature highlights (Find Mentors, Send Requests, Chat, Mentor Space)
- Partner showcase (Gaza Sky Geeks)
- Interactive FAQ section
- Smooth animations and modern UI design

### 🔐 Authentication System
- **Login Page**: Secure authentication with email/password
- **Multi-step Registration Process**:
  1. **Step 1**: Basic information (Full Name, Email, Password, Gender)
  2. **Step 2**: Role selection (Mentor or Mentee)
  3. **Step 3**: Complete profile with role-specific details
- **Password Reset**: Secure password recovery functionality
- **Session Management**: 30-minute session timeout with secure cookies

### 👤 Profile Management
#### Mentor Profile
- **Fields**: Full Name, Bio, LinkedIn URL, Expertise, Skills, Years of Experience, Availability, Rating, Review Count
- **Features**: Editable profile, rating system, review tracking
- **Visibility**: Shown to mentees in search results and recommendations

#### Mentee Profile  
- **Fields**: Full Name, Bio, LinkedIn URL, Field of Study, Interests, Goals
- **Features**: Editable profile, goal tracking
- **Purpose**: Used for mentor matching and recommendations

### 🗂️ Role-Based Dashboards
#### Mentor Dashboard
- Overview of received mentorship requests
- Quick actions: Accept/Decline requests

#### Mentee Dashboard
- Overview of sent mentorship requests and their status
- Mentor recommendations (filtered by expertise/skills)
- Progress tracking and request history
- Quick access to available mentors

### 💬 Real-Time Chat System
- **One-on-one messaging** between mentors and mentees
- **Features**:
  - List of active conversations
  - Message timestamps and read status
  - Real-time message delivery
  - Chat history per mentorship relationship
- **Integration**: Connected to mentorship requests for context

### 📬 Mentorship Request System
- **Structured Request Process**:
  - Mentees can send detailed requests to mentors
  - Include proposed date, time, session type, and personal message
  - Status tracking: Pending, Accepted, Declined
- **Request Management**:
  - Mentors can accept or decline requests
  - Automatic notifications for status changes
  - Request history and tracking

### 🌟 Mentor Space (Community Hub)
- **Community Features**:
  - Public posts and discussions
  - Post types: Questions, Advice, Experience sharing
  - Threaded comments and replies system
  - User-generated content with moderation
- **Engagement**: Like, comment, and share functionality
- **Categories**: Organized by topics and expertise areas

### 📊 Review & Rating System
- **Mentor Reviews**: Mentees can rate and review mentors
- **Rating System**: 5-star rating with written feedback
- **Statistics**: Average ratings and review counts displayed on profiles
- **Quality Assurance**: Helps maintain platform quality

### 🔔 Notification System
- **Real-time Notifications**:
  - Request approvals/rejections
  - New messages
  - Community interactions
  - System updates
- **Notification Types**: RequestApproved, RequestRejected, Message, etc.
- **User Experience**: Unread notification tracking and management

### 📄 Static Pages
- **About Page**: Mission, vision, and team information
- **Contact Page**: Support form and contact information
- **Privacy Policy**: Data protection and privacy information
- **Access Denied**: Error handling for unauthorized access

---

## 🗄️ Database Schema

### Core Entities
- **Users**: Central user table with authentication and profile data
- **MentorProfiles**: Extended mentor-specific information
- **MenteeProfiles**: Extended mentee-specific information
- **MentorshipRequests**: Request management and tracking
- **Chats**: Chat session management
- **Messages**: Individual message storage
- **MentorSpacePosts**: Community posts and discussions
- **MentorSpaceReplies**: Comments and replies to posts
- **MentorReviews**: Rating and review system
- **Notifications**: User notification management

### Key Relationships
### One-to-One (1:1)
- **User ↔ MentorProfile** (1:0..1) - Each user can have one mentor profile or none
- **User ↔ MenteeProfile** (1:0..1) - Each user can have one mentee profile or none

### One-to-Many (1:N)
- **User → MentorshipRequests** (as Mentor) - A mentor can have multiple mentorship requests
- **User → MentorshipRequests** (as Mentee) - A mentee can have multiple mentorship requests
- **User → Messages** (as Sender) - A user can send multiple messages
- **User → Messages** (as Receiver) - A user can receive multiple messages
- **User → MentorSpacePosts** - A user can create multiple posts
- **User → MentorSpaceReplies** - A user can create multiple replies
- **User → Notifications** - A user can have multiple notifications
- **User → Chats** - A user can have multiple chat sessions
---

## ⚙️ Setup & Installation

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation Steps
1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd MentorMate
   ```

2. **Configure Database Connection**
   - Update `appsettings.json` with your SQL Server connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=MentorMateDatabase;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=true"
   }
   ```

3. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Open your browser and navigate to: `https://localhost:5000` or `http://localhost:5000`

### Sample Data
The application automatically seeds sample data on first run:
- **Sample Mentor**: Omar Mentor (omar@example.com)
- **Sample Mentee**: Sara Mentee (sara@example.com)
- **Default Password**: Password123!

---

## 🎨 UI/UX Features
- **Responsive Design**: Mobile-first approach with Bootstrap 5
- **Modern UI**: Clean, professional interface with smooth animations
- **Accessibility**: ARIA labels and keyboard navigation support
- **Performance**: Optimized loading with CDN resources
- **User Experience**: Intuitive navigation and clear visual hierarchy

---

## 🔒 Security Features
- **Password Hashing**: Secure password storage using ASP.NET Identity hasher
- **Session Management**: Secure session handling with timeout
- **Input Validation**: Server-side and client-side validation
- **SQL Injection Protection**: Entity Framework parameterized queries
- **XSS Protection**: Razor view engine automatic encoding

---

## 👥 Development Team
- **Katya Kobari** – Backend Developer & Database Design
- **Alaa Abu Mussa** – Frontend Developer & UI/UX Design  
- **Ahmad Tayeh** – Frontend Developer & UI/UX Design 

---

## 📈 Future Enhancements
- Real-time notifications using SignalR
- Video call integration for mentorship sessions
- Advanced search and filtering capabilities
- Mobile application development
- Analytics dashboard for mentors
- Payment integration for premium features

---

## 🤝 Contributing
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

----

## 📞 Support
For support and questions, please contact the development team or create an issue in the repository.

----

## 🚩 Demo
https://drive.google.com/file/d/19ztGds8bKPUHTcjG5XI81cdNpZK3jN1n/view?usp=sharing
