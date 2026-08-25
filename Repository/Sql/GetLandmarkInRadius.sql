-- 1. Declare and set the target location once
DECLARE @TargetLocation AS geography = geography::Point(@Latitude, @Longitude, @Srid);

SELECT   l.landmark_id,
         l.description,
         l.city_id,
         CAST (l.location AS VARBINARY (MAX)) AS location,
         CAST (@TargetLocation.STDistance(l.location) AS DECIMAL (18, 2)) AS distance_to_city_centre,
         c.city_id,
         c.description,
         c.state_id,
         CAST (c.city_centre AS VARBINARY (MAX)) AS city_centre
FROM     landmark AS l
         INNER JOIN
         city AS c
         ON l.city_id = c.city_id
WHERE    @TargetLocation.STDistance(l.location) <= @RadiusInMeters
ORDER BY distance_to_city_centre ASC;
