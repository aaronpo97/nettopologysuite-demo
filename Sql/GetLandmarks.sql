SELECT
    l.landmark_id,
    l.description,
    l.city_id,
    CAST(l.location AS VARBINARY(MAX)) AS location,
    CAST(l.location.STDistance(c.city_centre) AS DECIMAL(18, 2)) AS distance_to_city_centre,
    c.city_id,
    c.description,
    c.state_id,
    CAST(c.city_centre AS VARBINARY(MAX)) AS city_centre
FROM landmark l
INNER JOIN city c ON l.city_id = c.city_id
ORDER BY l.landmark_id;
