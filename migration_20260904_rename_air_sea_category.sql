-- Migration: rename the "Air / Sea" form category to "Air / Sea / Land"
-- Date: 2026-09-04
-- Run against the testing Postgres database TOGETHER with the app build
-- whose FormCategoryConstants carries the new name (category dispatch and
-- exact-name matching in the app rely on the strings agreeing).
--
-- Data-only change: form categories are rows in a_tblcategory with
-- type = 'Form'. The column is varchar(20); 'Air / Sea / Land' is 16 chars.
-- The app's fuzzy dispatch (contains 'air' / 'sea') keeps working for old
-- app builds, but their exact-name lookups will not resolve until updated.

UPDATE public.a_tblcategory
SET category = 'Air / Sea / Land'
WHERE category = 'Air / Sea'
  AND UPPER(COALESCE(type, '')) = 'FORM';
