CREATE TABLE country (
   country_id  INT          ,
   description VARCHAR (MAX),
   CONSTRAINT pk_country PRIMARY KEY (country_id)
);

CREATE TABLE state (
   state_id    INT          ,
   description VARCHAR (MAX),
   country_id  INT          ,
   CONSTRAINT pk_state PRIMARY KEY (state_id),
   CONSTRAINT fk_state_country FOREIGN KEY (country_id) REFERENCES country (country_id)
);

CREATE NONCLUSTERED INDEX idx_state_country
   ON state(country_id);

CREATE TABLE city (
   city_id     INT          ,
   description VARCHAR (MAX) NOT NULL,
   city_centre geography     NOT NULL,
   state_id    INT           NOT NULL,
   CONSTRAINT pk_city PRIMARY KEY (city_id),
   CONSTRAINT fk_city_state FOREIGN KEY (state_id) REFERENCES state (state_id)
);

CREATE NONCLUSTERED INDEX idx_city_state
   ON city(state_id);

CREATE TABLE landmark (
   landmark_id INT          ,
   description VARCHAR (MAX) NOT NULL,
   city_id     INT           NOT NULL,
   location    geography     NOT NULL,
   CONSTRAINT pk_landmark PRIMARY KEY (landmark_id),
   CONSTRAINT fk_landmark_city FOREIGN KEY (city_id) REFERENCES city (city_id)
);
