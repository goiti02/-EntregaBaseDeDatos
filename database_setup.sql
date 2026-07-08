CREATE DATABASE IF NOT EXISTS brawl_stars;
USE brawl_stars;

CREATE TABLE BRAWLERS (
    brawler_id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(50) NOT NULL
);

CREATE TABLE USER_BRAWLERS (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id VARCHAR(50),
    brawler_id INT,
    level INT DEFAULT 1,
    trophies INT DEFAULT 0,
    FOREIGN KEY (brawler_id) REFERENCES BRAWLERS(brawler_id)
);

-- Inserta un par de datos de prueba
INSERT INTO BRAWLERS (name) VALUES ('Shelly'), ('Nita'), ('Colt'), ('Bull'), ('Brock');
INSERT INTO USER_BRAWLERS (user_id, brawler_id, level, trophies) VALUES ('JY_USER', 1, 8, 929), ('JY_USER', 2, 9, 928);