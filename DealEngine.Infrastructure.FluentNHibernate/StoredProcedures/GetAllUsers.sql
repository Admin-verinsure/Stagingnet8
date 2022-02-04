-- PROCEDURE: public.GetAllUsers()

-- DROP PROCEDURE IF EXISTS public."GetAllUsers"();

CREATE OR REPLACE PROCEDURE public."GetAllUsers"(
	)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
SELECT * FROM public."User";
END;
$BODY$;
