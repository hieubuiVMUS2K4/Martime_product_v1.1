import psycopg2
connection_string = 'host=localhost port=5433 dbname=maritime_edge user=edge_user password=edge_password'
try:
    conn = psycopg2.connect(connection_string)
    cursor = conn.cursor()
    cursor.execute("SELECT data_type FROM information_schema.columns WHERE table_name = 'report_types' AND column_name = 'id';")
    cols = cursor.fetchall()
    print('report_types id type:', cols[0][0])
except Exception as e:
    print('Error:', e)
