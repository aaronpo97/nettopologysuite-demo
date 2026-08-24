SELECT
    c.city_id,
    c.description,
    c.state_id,
    CAST(c.city_centre AS VARBINARY(MAX)) AS city_centre,
    s.state_id,
    s.description,
    s.country_id,
    co.country_id,
    co.description
FROM city c
INNER JOIN state s ON c.state_id = s.state_id
INNER JOIN country co ON s.country_id = co.country_id
ORDER BY c.city_id;
