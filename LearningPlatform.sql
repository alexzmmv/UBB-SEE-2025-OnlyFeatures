DROP TABLE IF EXISTS DailyLoginReward;
DROP TABLE IF EXISTS UserModuleUnlock;
DROP TABLE IF EXISTS CourseTimer;
DROP TABLE IF EXISTS UserPictureCoins;
DROP TABLE IF EXISTS UserPictures;
DROP TABLE IF EXISTS UserCoins;
DROP TABLE IF EXISTS UserProgress;
DROP TABLE IF EXISTS Enrollment;
DROP TABLE IF EXISTS CourseTags;
DROP TABLE IF EXISTS Tags;
DROP TABLE IF EXISTS Modules;
DROP TABLE IF EXISTS Courses;
DROP TABLE IF EXISTS CourseTypes;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS ModuleImages;

CREATE TABLE Users (
    UserId INT PRIMARY KEY,
    Username VARCHAR(255) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    CreatedAt DATETIME2
);

CREATE TABLE CourseTypes (
    TypeId INT PRIMARY KEY,
    TypeName VARCHAR(100) NOT NULL UNIQUE,
    Price DECIMAL(10,2) NOT NULL CHECK (Price >= 0)
);

CREATE TABLE Courses (
    CourseId INT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    TypeId INT NOT NULL,
    CreatedAt DATETIME2,
    DifficultyLevel INT NOT NULL,
    TimerDurationMinutes INT NOT NULL,
    TimerCompletionReward DECIMAL(10,2) NOT NULL,
    CompletionReward DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (TypeId) REFERENCES CourseTypes(TypeId) ON DELETE CASCADE
);

CREATE TABLE Modules (
    ModuleId INT PRIMARY KEY,
    CourseId INT NOT NULL,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    Position INT NOT NULL,
    IsBonusModule BIT NOT NULL DEFAULT 0,
    UnlockCost DECIMAL(10,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId) ON DELETE CASCADE
);

CREATE TABLE Tags (
    TagId INT PRIMARY KEY,
    TagName VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE CourseTags (
    CourseId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (CourseId, TagId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES Tags(TagId) ON DELETE CASCADE
);

CREATE TABLE Enrollment (
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    EnrolledAt DATETIME2,
    PRIMARY KEY (UserId, CourseId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId) ON DELETE CASCADE
);

CREATE TABLE UserProgress (
    ProgressId INT PRIMARY KEY,
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    ModuleId INT NOT NULL,
    ProgressPercentage INT CHECK (ProgressPercentage BETWEEN 0 AND 100),
    LastUpdated DATETIME2,
    UNIQUE (UserId, CourseId, ModuleId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId) ON DELETE NO ACTION,
    FOREIGN KEY (ModuleId) REFERENCES Modules(ModuleId) ON DELETE NO ACTION
);

CREATE TABLE UserCoins (
    UserId INT PRIMARY KEY,
    CoinBalance DECIMAL(10,2) DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE TABLE UserPictures (
    PictureId INT PRIMARY KEY,
    UserId INT NOT NULL,
    ImageUrl VARCHAR(500) NOT NULL,
    UploadedAt DATETIME2,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);

CREATE TABLE UserPictureCoins (
    UserId INT NOT NULL,
    PictureId INT NOT NULL,
    CoinsReceived DECIMAL(10,2) DEFAULT 0,
    PRIMARY KEY (UserId, PictureId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (PictureId) REFERENCES UserPictures(PictureId) ON DELETE CASCADE
);

CREATE TABLE CourseTimer (
    TimerId INT PRIMARY KEY,
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    ElapsedTimeMinutes INT NOT NULL DEFAULT 0,
    LastUpdated DATETIME2,
    IsCompleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId) ON DELETE CASCADE
);

CREATE TABLE UserModuleUnlock (
    UserId INT NOT NULL,
    ModuleId INT NOT NULL,
    UnlockedAt DATETIME2,
    PRIMARY KEY (UserId, ModuleId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (ModuleId) REFERENCES Modules(ModuleId) ON DELETE CASCADE
);

CREATE TABLE DailyLoginReward (
    RewardId INT PRIMARY KEY,
    UserId INT NOT NULL,
    RewardDate DATETIME2,
    CoinsReceived DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE TABLE ModuleImages (
    ModuleId INT,
    PictureId INT,
    PRIMARY KEY (ModuleId, PictureId),
    FOREIGN KEY (ModuleId) REFERENCES Modules(ModuleId) ON DELETE CASCADE,
    FOREIGN KEY (PictureId) REFERENCES UserPictures(PictureId) ON DELETE CASCADE
);

CREATE INDEX idx_course_title ON Courses(Title);
CREATE INDEX idx_course_tags ON CourseTags(TagId);
CREATE INDEX idx_user_progress ON UserProgress(UserId, CourseId);
CREATE INDEX idx_course_timer ON CourseTimer(UserId, CourseId);
CREATE INDEX idx_daily_login ON DailyLoginReward(UserId, RewardDate);


-- Tags Table - Topics for courses
INSERT INTO Tags (TagId, TagName) VALUES 
(1, 'Programming'),
(2, 'Language Learning'),
(3, 'Mathematics'),
(4, 'Science'),
(5, 'History');

-- CourseTypes Table - Free or Premium courses with prices
INSERT INTO CourseTypes (TypeId, TypeName, Price) VALUES 
(1, 'Free', 0.00),
(2, 'Premium', 50.00);

-- Courses Table - Main course information with new fields
INSERT INTO Courses (CourseId, Title, Description, TypeId, CreatedAt, DifficultyLevel, TimerDurationMinutes, TimerCompletionReward, CompletionReward) VALUES 
(1, 'Introduction to Python', 'Learn the basics of Python programming language', 1, '2025-01-15 10:00:00', 1, 120, 15.00, 50.00),
(2, 'Advanced JavaScript', 'Master JavaScript for web development', 2, '2025-01-20 14:30:00', 3, 180, 25.00, 75.00),
(3, 'Spanish for Beginners', 'Learn essential Spanish vocabulary and grammar', 2, '2025-02-01 09:15:00', 1, 90, 10.00, 30.00),
(4, 'Basic Mathematics', 'Fundamentals of arithmetic and algebra', 1, '2025-02-10 11:45:00', 1, 60, 5.00, 20.00),
(5, 'Data Science Fundamentals', 'Introduction to data analysis and visualization', 2, '2025-02-15 13:20:00', 2, 150, 20.00, 60.00);

-- CourseTags Table - Mapping between courses and tags
INSERT INTO CourseTags (CourseId, TagId) VALUES 
(1, 1), 
(2, 1), 
(3, 2), 
(4, 3), 
(5, 1), 
(5, 3), 
(5, 4); 

-- Modules Table - Course modules with new fields
INSERT INTO Modules (ModuleId, CourseId, Title, Description, Position, IsBonusModule, UnlockCost) VALUES 
(1, 1, 'Python Basics', 'Variables, data types, and basic operations', 1, 0, 0.00),
(2, 1, 'Control Flow', 'Conditionals and loops in Python', 2, 0, 0.00),
(3, 1, 'Functions', 'Creating and using functions', 3, 0, 0.00),
(4, 1, 'Python Bonus: Advanced Topics', 'Extra content on advanced Python features', 4, 1, 25.00),
(5, 2, 'JavaScript Fundamentals', 'Basic syntax and concepts', 1, 0, 0.00),
(6, 2, 'DOM Manipulation', 'Interacting with webpage elements', 2, 0, 0.00),
(7, 2, 'Asynchronous JavaScript', 'Promises and async/await', 3, 0, 0.00),
(8, 3, 'Spanish Greetings', 'Basic phrases and introductions', 1, 0, 0.00),
(9, 3, 'Spanish Numbers and Colors', 'Essential vocabulary', 2, 0, 0.00),
(10, 3, 'Basic Conversation', 'Putting it all together', 3, 0, 0.00),
(11, 4, 'Numbers and Operations', 'Understanding arithmetic', 1, 0, 0.00),
(12, 4, 'Introduction to Algebra', 'Variables and equations', 2, 0, 0.00),
(13, 5, 'Introduction to Data Analysis', 'Basic concepts and tools', 1, 0, 0.00),
(14, 5, 'Data Visualization', 'Creating effective charts and graphs', 2, 0, 0.00),
(15, 5, 'Statistical Analysis', 'Basic statistics for data interpretation', 3, 0, 0.00),
(16, 5, 'Advanced ML Techniques', 'Machine learning for data science', 4, 1, 40.00);

-- Users Table - Sample users
INSERT INTO Users (UserId, Username, Email, CreatedAt) VALUES 
(1, 'john_doe', 'john@example.com', '2024-01-02 00:00:00'), 
(2, 'jane_smith', 'jane@example.com', '2024-01-05 10:30:00'),
(3, 'bob_johnson', 'bob@example.com', '2024-01-10 08:00:00'),
(4, 'alice_chen', 'alice@example.com', '2024-01-12 15:20:00'),
(5, 'david_kim', 'david@example.com', '2024-01-15 11:45:00');

-- UserCoins Table - User coin balances with decimal
INSERT INTO UserCoins (UserId, CoinBalance) VALUES 
(1, 250.50),
(2, 175.25),
(3, 300.00),
(4, 125.75),
(5, 450.00);

-- UserPictures Table
INSERT INTO UserPictures (PictureId, UserId, ImageUrl, UploadedAt) VALUES 
(1, 1, 'https://example.com/modules/python_variables.jpg', '2025-01-15 10:10:00'), 
(2, 1, 'https://example.com/modules/python_functions.jpg', '2025-01-15 10:15:00'), 
(3, 2, 'https://example.com/modules/javascript_dom.jpg', '2025-01-20 15:00:00'),
(4, 2, 'https://example.com/modules/spanish_greetings.jpg', '2025-02-01 09:45:00'),
(5, 3, 'https://example.com/modules/math_algebra.jpg', '2025-02-10 12:20:00'),
(6, 3, 'https://example.com/modules/data_visualization.jpg', '2025-02-15 13:45:00'); 

-- Enrollment Table
INSERT INTO Enrollment (UserId, CourseId, EnrolledAt) VALUES 
(1, 1, '2025-01-16 11:10:00'), 
(1, 3, '2025-02-03 12:30:00'), 
(2, 2, '2025-01-22 10:00:00'), 
(2, 4, '2025-02-12 14:30:00'), 
(3, 1, '2025-01-18 09:45:00'),
(3, 5, '2025-02-17 13:15:00'),
(4, 2, '2025-01-25 16:20:00'),
(4, 5, '2025-02-20 09:10:00'),
(5, 1, '2025-01-17 14:05:00'),
(5, 3, '2025-02-05 10:30:00'),
(5, 5, '2025-02-18 11:15:00'); 

--UserProgress Table
INSERT INTO UserProgress (ProgressId, UserId, CourseId, ModuleId, ProgressPercentage, LastUpdated) VALUES 
(1, 1, 1, 1, 100, '2025-01-17 16:00:00'),
(2, 1, 1, 2, 100, '2025-01-19 14:45:00'),
(3, 1, 1, 3, 75, '2025-01-21 09:10:00'),  
(4, 1, 3, 8, 100, '2025-02-05 13:30:00'),
(5, 2, 2, 5, 100, '2025-01-25 15:30:00'),
(6, 2, 2, 6, 50, '2025-01-28 16:00:00'),  
(7, 2, 4, 11, 100, '2025-02-15 13:20:00'),
(8, 3, 1, 1, 100, '2025-01-20 14:30:00'), 
(9, 3, 1, 2, 100, '2025-01-22 16:00:00'), 
(10, 3, 1, 3, 100, '2025-01-25 10:00:00'), 
(11, 3, 5, 13, 100, '2025-02-18 16:30:00'),
(12, 4, 2, 5, 100, '2025-01-26 10:15:00'),
(13, 4, 2, 6, 85, '2025-01-29 11:20:00'),
(14, 4, 5, 13, 100, '2025-02-22 14:30:00'),
(15, 5, 1, 1, 100, '2025-01-19 09:45:00'),
(16, 5, 1, 2, 100, '2025-01-21 13:20:00'),
(17, 5, 3, 8, 100, '2025-02-07 15:10:00'),
(18, 5, 5, 13, 75, '2025-02-20 10:30:00');

-- UserPictureCoins Table - With decimal coins received
INSERT INTO UserPictureCoins (UserId, PictureId, CoinsReceived) VALUES 
(1, 1, 5.00),  
(1, 2, 10.00), 
(2, 3, 5.00), 
(2, 4, 15.00),
(3, 5, 10.00),
(3, 6, 20.00),
(4, 3, 7.50),
(5, 1, 5.00),
(5, 2, 10.00);

-- ModuleImages Table
INSERT INTO ModuleImages (ModuleId, PictureId) VALUES
(1, 1), 
(3, 2), 
(6, 3),
(8, 4), 
(12, 5), 
(14, 6);

-- CourseTimer Table
INSERT INTO CourseTimer (TimerId, UserId, CourseId, ElapsedTimeMinutes, LastUpdated, IsCompleted) VALUES
(1, 1, 1, 95, '2025-01-21 16:30:00', 0),
(2, 1, 3, 60, '2025-02-06 14:15:00', 0),
(3, 2, 2, 170, '2025-01-28 17:00:00', 0),
(4, 2, 4, 60, '2025-02-15 14:30:00', 1),
(5, 3, 1, 120, '2025-01-25 11:45:00', 1),
(6, 3, 5, 75, '2025-02-19 10:20:00', 0),
(7, 4, 2, 140, '2025-01-29 12:10:00', 0),
(8, 4, 5, 60, '2025-02-22 15:45:00', 0),
(9, 5, 1, 120, '2025-01-21 15:00:00', 1),
(10, 5, 3, 90, '2025-02-08 11:30:00', 1),
(11, 5, 5, 45, '2025-02-20 12:15:00', 0);

-- UserModuleUnlock Table
INSERT INTO UserModuleUnlock (UserId, ModuleId, UnlockedAt) VALUES
(1, 4, '2025-01-20 10:45:00'),
(2, 7, '2025-01-27 13:20:00'),
(3, 4, '2025-01-23 09:30:00'),
(3, 16, '2025-02-19 14:15:00'),
(4, 16, '2025-02-23 11:00:00'),
(5, 4, '2025-01-22 16:40:00'),
(5, 16, '2025-02-21 09:15:00');

-- DailyLoginReward Table
INSERT INTO DailyLoginReward (RewardId, UserId, RewardDate, CoinsReceived) VALUES
(1, 1, '2025-01-16 09:30:00', 5.00),
(2, 1, '2025-01-17 10:15:00', 5.50),
(3, 1, '2025-01-18 11:00:00', 6.00),
(4, 1, '2025-01-19 09:45:00', 6.50),
(5, 1, '2025-01-20 10:30:00', 7.00),
(6, 2, '2025-01-22 08:15:00', 5.00),
(7, 2, '2025-01-23 09:00:00', 5.50),
(8, 2, '2025-01-24 08:30:00', 6.00),
(9, 3, '2025-01-18 14:20:00', 5.00),
(10, 3, '2025-01-19 15:10:00', 5.50),
(11, 3, '2025-01-20 14:45:00', 6.00),
(12, 3, '2025-01-21 16:00:00', 6.50),
(13, 4, '2025-01-25 10:00:00', 5.00),
(14, 4, '2025-01-26 11:30:00', 5.50),
(15, 4, '2025-01-27 09:15:00', 6.00),
(16, 4, '2025-01-28 10:45:00', 6.50),
(17, 5, '2025-01-17 08:30:00', 5.00),
(18, 5, '2025-01-18 09:15:00', 5.50),
(19, 5, '2025-01-19 08:45:00', 6.00),
(20, 5, '2025-01-20 09:30:00', 6.50),
(21, 5, '2025-01-21 08:15:00', 7.00);