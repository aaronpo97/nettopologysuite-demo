USE master;
GO

IF (SELECT NAME
    FROM sys.databases
    WHERE NAME = 'NetTopologySuiteDemo') IS NOT NULL
ALTER DATABASE nettopologysuitedemo
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE;
GO

DROP DATABASE nettopologysuitedemo;
GO

CREATE DATABASE nettopologysuitedemo;
GO

USE nettopologysuitedemo;
GO

CREATE TABLE country
(
    country_id  INT,
    description VARCHAR(MAX),
    CONSTRAINT pk_country PRIMARY KEY (country_id)
);

CREATE TABLE state
(
    state_id    INT,
    description VARCHAR(MAX),
    country_id  INT,
    CONSTRAINT pk_state PRIMARY KEY (state_id),
    CONSTRAINT fk_state_country FOREIGN KEY (country_id) REFERENCES country (country_id)
);

CREATE NONCLUSTERED INDEX idx_state_country ON state (country_id);

CREATE TABLE city
(
    city_id     INT,
    description VARCHAR(MAX) NOT NULL,
    city_centre GEOGRAPHY    NOT NULL,
    state_id    INT          NOT NULL,
    CONSTRAINT pk_city PRIMARY KEY (city_id),
    CONSTRAINT fk_city_state FOREIGN KEY (state_id) REFERENCES state (state_id)
);

CREATE NONCLUSTERED INDEX idx_city_state ON city (state_id);

CREATE TABLE landmark
(
    landmark_id INT,
    description VARCHAR(MAX) NOT NULL,
    city_id     INT          NOT NULL,
    location    GEOGRAPHY    NOT NULL,
    CONSTRAINT pk_landmark PRIMARY KEY (landmark_id),
    CONSTRAINT fk_landmark_city FOREIGN KEY (city_id) REFERENCES city (city_id)
);

CREATE NONCLUSTERED INDEX idx_landmark_city ON landmark (city_id);
GO

-- Seed data for country / state / city / landmark
-- Demo data with real-world coordinates (WGS84 / SRID 4326)
DECLARE @SRID INT = 4326;

INSERT INTO country (country_id, description)
VALUES (1, 'United States'),
       (2, 'Canada'),
       (3, 'Mexico'),
       (4, 'United Kingdom'),
       (5, 'France'),
       (6, 'Germany'),
       (7, 'Australia'),
       (8, 'Japan'),
       (9, 'Brazil'),
       (10, 'India');

INSERT INTO state (state_id, description, country_id)
VALUES (1, 'California', 1),
       (2, 'New York', 1),
       (3, 'Texas', 1),
       (4, 'Florida', 1),
       (5, 'Illinois', 1),
       (6, 'Ontario', 2),
       (7, 'Quebec', 2),
       (8, 'British Columbia', 2),
       (9, 'Jalisco', 3),
       (10, 'Nuevo Leon', 3),
       (11, 'Ciudad de Mexico', 3),
       (12, 'England', 4),
       (13, 'Scotland', 4),
       (14, 'Ile-de-France', 5),
       (15, 'Provence-Alpes-Cote d''Azur', 5),
       (16, 'Bavaria', 6),
       (17, 'Berlin', 6),
       (18, 'New South Wales', 7),
       (19, 'Victoria', 7),
       (20, 'Tokyo', 8),
       (21, 'Osaka', 8),
       (22, 'Sao Paulo', 9),
       (23, 'Rio de Janeiro', 9),
       (24, 'Maharashtra', 10),
       (25, 'Delhi', 10);

INSERT INTO city (city_id, description, city_centre, state_id)
VALUES (1, 'Los Angeles', geography::Point(34.0522, -118.2437, @SRID), 1),
       (2, 'San Francisco', geography::Point(37.7749, -122.4194, @SRID), 1),
       (3, 'San Diego', geography::Point(32.7157, -117.1611, @SRID), 1),
       (4, 'New York City', geography::Point(40.7128, -74.006, @SRID), 2),
       (5, 'Buffalo', geography::Point(42.8864, -78.8784, @SRID), 2),
       (6, 'Houston', geography::Point(29.7604, -95.3698, @SRID), 3),
       (7, 'Austin', geography::Point(30.2672, -97.7431, @SRID), 3),
       (8, 'Dallas', geography::Point(32.7767, -96.797, @SRID), 3),
       (9, 'Miami', geography::Point(25.7617, -80.1918, @SRID), 4),
       (10, 'Orlando', geography::Point(28.5383, -81.3792, @SRID), 4),
       (11, 'Chicago', geography::Point(41.8781, -87.6298, @SRID), 5),
       (12, 'Toronto', geography::Point(43.6532, -79.3832, @SRID), 6),
       (13, 'Ottawa', geography::Point(45.4215, -75.6972, @SRID), 6),
       (14, 'Montreal', geography::Point(45.5017, -73.5673, @SRID), 7),
       (15, 'Quebec City', geography::Point(46.8139, -71.208, @SRID), 7),
       (16, 'Vancouver', geography::Point(49.2827, -123.1207, @SRID), 8),
       (17, 'Guadalajara', geography::Point(20.6597, -103.3496, @SRID), 9),
       (18, 'Monterrey', geography::Point(25.6866, -100.3161, @SRID), 10),
       (19, 'Mexico City', geography::Point(19.4326, -99.1332, @SRID), 11),
       (20, 'London', geography::Point(51.5074, -0.1278, @SRID), 12),
       (21, 'Manchester', geography::Point(53.4808, -2.2426, @SRID), 12),
       (22, 'Edinburgh', geography::Point(55.9533, -3.1883, @SRID), 13),
       (23, 'Paris', geography::Point(48.8566, 2.3522, @SRID), 14),
       (24, 'Nice', geography::Point(43.7102, 7.262, @SRID), 15),
       (25, 'Marseille', geography::Point(43.2965, 5.3698, @SRID), 15),
       (26, 'Munich', geography::Point(48.1351, 11.582, @SRID), 16),
       (27, 'Berlin', geography::Point(52.52, 13.405, @SRID), 17),
       (28, 'Sydney', geography::Point(-33.8688, 151.2093, @SRID), 18),
       (29, 'Melbourne', geography::Point(-37.8136, 144.9631, @SRID), 19),
       (30, 'Tokyo', geography::Point(35.6762, 139.6503, @SRID), 20),
       (31, 'Osaka', geography::Point(34.6937, 135.5023, @SRID), 21),
       (32, 'Sao Paulo', geography::Point(-23.5505, -46.6333, @SRID), 22),
       (33, 'Rio de Janeiro', geography::Point(-22.9068, -43.1729, @SRID), 23),
       (34, 'Mumbai', geography::Point(19.076, 72.8777, @SRID), 24),
       (35, 'New Delhi', geography::Point(28.6139, 77.209, @SRID), 25),
       (36, 'Niagara Falls', geography::Point(43.0962, -79.0377, @SRID), 2);

INSERT INTO landmark (landmark_id, description, city_id, location)
VALUES (1, 'Hollywood Sign', 1, geography::Point(34.1341, -118.3215, @SRID)),
       (2, 'Griffith Observatory', 1, geography::Point(34.1184, -118.3004, @SRID)),
       (3, 'Santa Monica Pier', 1, geography::Point(34.0100, -118.4963, @SRID)),
       (4, 'Venice Beach', 1, geography::Point(33.9850, -118.4695, @SRID)),
       (5, 'Golden Gate Bridge', 2, geography::Point(37.8199, -122.4783, @SRID)),
       (6, 'Alcatraz Island', 2, geography::Point(37.8267, -122.4230, @SRID)),
       (7, 'Fisherman''s Wharf', 2, geography::Point(37.8080, -122.4177, @SRID)),
       (8, 'USS Midway Museum', 3, geography::Point(32.7137, -117.1751, @SRID)),
       (9, 'Balboa Park', 3, geography::Point(32.7341, -117.1449, @SRID)),
       (10, 'Statue of Liberty', 4, geography::Point(40.6892, -74.0445, @SRID)),
       (11, 'Empire State Building', 4, geography::Point(40.7484, -73.9857, @SRID)),
       (12, 'Central Park', 4, geography::Point(40.7829, -73.9654, @SRID)),
       (13, 'Times Square', 4, geography::Point(40.7580, -73.9855, @SRID)),
       (14, 'Buffalo City Hall', 5, geography::Point(42.8867, -78.8784, @SRID)),
       (15, 'Niagara Falls State Park', 36, geography::Point(43.0828, -79.0742, @SRID)),
       (16, 'Space Center Houston', 6, geography::Point(29.5518, -95.0982, @SRID)),
       (17, 'Houston Zoo', 6, geography::Point(29.7160, -95.3908, @SRID)),
       (18, 'Texas State Capitol', 7, geography::Point(30.2747, -97.7404, @SRID)),
       (19, 'Lady Bird Lake', 7, geography::Point(30.2500, -97.7500, @SRID)),
       (20, 'Reunion Tower', 8, geography::Point(32.7753, -96.8089, @SRID)),
       (21, 'Dallas Arboretum', 8, geography::Point(32.8214, -96.7180, @SRID)),
       (22, 'South Beach', 9, geography::Point(25.7907, -80.1300, @SRID)),
       (23, 'Vizcaya Museum and Gardens', 9, geography::Point(25.7448, -80.2106, @SRID)),
       (24, 'Walt Disney World', 10, geography::Point(28.3852, -81.5639, @SRID)),
       (25, 'Universal Studios Florida', 10, geography::Point(28.4743, -81.4677, @SRID)),
       (26, 'Willis Tower', 11, geography::Point(41.8789, -87.6359, @SRID)),
       (27, 'Millennium Park', 11, geography::Point(41.8826, -87.6226, @SRID)),
       (28, 'Navy Pier', 11, geography::Point(41.8917, -87.6086, @SRID)),
       (29, 'CN Tower', 12, geography::Point(43.6426, -79.3871, @SRID)),
       (30, 'Royal Ontario Museum', 12, geography::Point(43.6677, -79.3948, @SRID)),
       (31, 'Parliament Hill', 13, geography::Point(45.4236, -75.7009, @SRID)),
       (32, 'Rideau Canal', 13, geography::Point(45.4231, -75.6919, @SRID)),
       (33, 'Notre-Dame Basilica', 14, geography::Point(45.5046, -73.5563, @SRID)),
       (34, 'Old Montreal', 14, geography::Point(45.5075, -73.5541, @SRID)),
       (35, 'Chateau Frontenac', 15, geography::Point(46.8123, -71.2050, @SRID)),
       (36, 'Stanley Park', 16, geography::Point(49.3017, -123.1444, @SRID)),
       (37, 'Capilano Suspension Bridge', 16, geography::Point(49.3429, -123.1150, @SRID)),
       (38, 'Guadalajara Cathedral', 17, geography::Point(20.6767, -103.3475, @SRID)),
       (39, 'Hospicio Cabanas', 17, geography::Point(20.6775, -103.3378, @SRID)),
       (40, 'Cerro de la Silla', 18, geography::Point(25.6392, -100.2464, @SRID)),
       (41, 'Macroplaza', 18, geography::Point(25.6692, -100.3099, @SRID)),
       (42, 'Chapultepec Castle', 19, geography::Point(19.4204, -99.1822, @SRID)),
       (43, 'Zocalo', 19, geography::Point(19.4326, -99.1332, @SRID)),
       (44, 'Frida Kahlo Museum', 19, geography::Point(19.3550, -99.1624, @SRID)),
       (45, 'Big Ben', 20, geography::Point(51.5007, -0.1246, @SRID)),
       (46, 'Tower of London', 20, geography::Point(51.5081, -0.0759, @SRID)),
       (47, 'British Museum', 20, geography::Point(51.5194, -0.1270, @SRID)),
       (48, 'London Eye', 20, geography::Point(51.5033, -0.1195, @SRID)),
       (49, 'Old Trafford', 21, geography::Point(53.4631, -2.2913, @SRID)),
       (50, 'Edinburgh Castle', 22, geography::Point(55.9486, -3.1999, @SRID)),
       (51, 'Arthur''s Seat', 22, geography::Point(55.9445, -3.1618, @SRID)),
       (52, 'Eiffel Tower', 23, geography::Point(48.8584, 2.2945, @SRID)),
       (53, 'Louvre Museum', 23, geography::Point(48.8606, 2.3376, @SRID)),
       (54, 'Notre-Dame Cathedral', 23, geography::Point(48.8530, 2.3499, @SRID)),
       (55, 'Arc de Triomphe', 23, geography::Point(48.8738, 2.2950, @SRID)),
       (56, 'Promenade des Anglais', 24, geography::Point(43.6950, 7.2650, @SRID)),
       (57, 'Old Port of Marseille', 25, geography::Point(43.2951, 5.3739, @SRID)),
       (58, 'Marienplatz', 26, geography::Point(48.1374, 11.5755, @SRID)),
       (59, 'Nymphenburg Palace', 26, geography::Point(48.1583, 11.5033, @SRID)),
       (60, 'Brandenburg Gate', 27, geography::Point(52.5163, 13.3777, @SRID)),
       (61, 'Berlin Wall Memorial', 27, geography::Point(52.5351, 13.3903, @SRID)),
       (62, 'Museum Island', 27, geography::Point(52.5169, 13.4016, @SRID)),
       (63, 'Sydney Opera House', 28, geography::Point(-33.8568, 151.2153, @SRID)),
       (64, 'Sydney Harbour Bridge', 28, geography::Point(-33.8523, 151.2108, @SRID)),
       (65, 'Bondi Beach', 28, geography::Point(-33.8908, 151.2743, @SRID)),
       (66, 'Federation Square', 29, geography::Point(-37.8180, 144.9691, @SRID)),
       (67, 'Royal Botanic Gardens', 29, geography::Point(-37.8304, 144.9796, @SRID)),
       (68, 'Tokyo Tower', 30, geography::Point(35.6586, 139.7454, @SRID)),
       (69, 'Senso-ji Temple', 30, geography::Point(35.7148, 139.7967, @SRID)),
       (70, 'Shibuya Crossing', 30, geography::Point(35.6595, 139.7005, @SRID)),
       (71, 'Osaka Castle', 31, geography::Point(34.6873, 135.5259, @SRID)),
       (72, 'Dotonbori', 31, geography::Point(34.6687, 135.5013, @SRID)),
       (73, 'Paulista Avenue', 32, geography::Point(-23.5613, -46.6559, @SRID)),
       (74, 'Ibirapuera Park', 32, geography::Point(-23.5874, -46.6576, @SRID)),
       (75, 'Christ the Redeemer', 33, geography::Point(-22.9519, -43.2105, @SRID)),
       (76, 'Copacabana Beach', 33, geography::Point(-22.9711, -43.1822, @SRID)),
       (77, 'Sugarloaf Mountain', 33, geography::Point(-22.9492, -43.1545, @SRID)),
       (78, 'Gateway of India', 34, geography::Point(18.9220, 72.8347, @SRID)),
       (79, 'Marine Drive', 34, geography::Point(18.9440, 72.8232, @SRID)),
       (80, 'India Gate', 35, geography::Point(28.6129, 77.2295, @SRID)),
       (81, 'Red Fort', 35, geography::Point(28.6562, 77.2410, @SRID)),
       (82, 'Qutub Minar', 35, geography::Point(28.5245, 77.1855, @SRID));
