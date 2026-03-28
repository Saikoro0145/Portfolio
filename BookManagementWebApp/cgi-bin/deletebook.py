#!/usr/bin/env python3
import sys
import io
import os
import urllib.parse
import sqlite3
import searchresult as search

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
DB_PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "bookdb.db"))

def GetParams():
    form = {}
    content_length = os.environ.get('CONTENT_LENGTH')
    if content_length:
        body = sys.stdin.read(int(content_length))
        params = urllib.parse.parse_qs(body, keep_blank_values=True)
        for key, values in params.items():
            form[key] = values[0]
    return form

def PrintErrorPage(message):
    print("Content-Type: text/html; charset=utf-8\n")
    print(f"""<html>
  <head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>削除エラー</title>
  </head>
  <body>
    <p>{message}</p>
    <a href="/searchform.html">ホーム画面に戻る</a>
  </body>
</html>
""")

def DeleteBook(book_id):
    con = sqlite3.connect(DB_PATH)
    cur = con.cursor()
    try:
        cur.execute("delete from BOOKLIST where ID = ?", (book_id,))
        con.commit()
    finally:
        con.close()

def main():
    params = GetParams()
    book_id = params.get('id', '').strip()
    query = params.get('param1', '')
    if not book_id.isdigit():
        PrintErrorPage("削除対象のIDが不正です．")
        return

    try:
        DeleteBook(int(book_id))
    except sqlite3.Error as e:
        PrintErrorPage(f"Error occurred: {e.args[0]}")
        return

    keywords = search.NormalizeKeywords(query)
    rows = search.SearchInDB(keywords)
    search.PrintHtmlPage(query, rows)

if __name__ == "__main__":
    main()
