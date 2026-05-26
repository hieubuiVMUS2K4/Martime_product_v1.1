import psycopg2
import sys

connection_string = "host=localhost port=5433 dbname=maritime_edge user=edge_user password=edge_password"

try:
    conn = psycopg2.connect(connection_string)
    conn.autocommit = True
    cursor = conn.cursor()
    
    with open("f:/NCKH/Product/Martime_product_v1.1/edge_product/scripts/seed_noon_reports.sql", "r", encoding="utf-8") as f:
        sql = f.read()
        
    cursor.execute(sql)
    print("Seed data successfully inserted!")
    
except Exception as e:
    print(f"Error: {e}")
finally:
    if 'conn' in locals() and conn:
        conn.close()
