import psycopg2
connection_string = 'host=localhost port=5433 dbname=maritime_edge user=edge_user password=edge_password'
try:
    conn = psycopg2.connect(connection_string)
    cursor = conn.cursor()
    cursor.execute("SELECT column_name FROM information_schema.columns WHERE table_name = 'maritime_reports';")
    cols = cursor.fetchall()
    print('maritime_reports:', [c[0] for c in cols])
    cursor.execute("SELECT column_name FROM information_schema.columns WHERE table_name = 'report_types';")
    cols = cursor.fetchall()
    print('report_types:', [c[0] for c in cols])
except Exception as e:
    print('Error:', e)
