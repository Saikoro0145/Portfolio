#!/usr/bin/env python3
import sys
import io
import os
import urllib.parse
import sqlite3

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
    <title>追加エラー</title>
  </head>
  <body>
    <p>{message}</p>
    <a href="/searchform.html">ホーム画面に戻る</a>
  </body>
</html>
""")

def GetNextId(cur):
    cur.execute("select ID from BOOKLIST order by ID")
    next_id = 1
    for (book_id,) in cur.fetchall():
        if book_id == next_id:
            next_id += 1
        elif book_id > next_id:
            break
    return next_id

def main():
    params = GetParams()
    title = params.get('title', '').strip()
    author = params.get('author', '').strip()
    publisher = params.get('publisher', '').strip()
    isbn = params.get('isbn', '').strip()

    # サーバー側でも検証
    if not title or not author or not publisher or not isbn:
        PrintErrorPage("タイトル，著者，出版社，ISBN を入力して下さい．")
        return
    if not isbn.isdigit() or len(isbn) != 13:
        PrintErrorPage("ISBN は13桁の数字で入力して下さい．")
        return

    con = sqlite3.connect(DB_PATH)
    cur = con.cursor()
    try:
        cur.execute("select count(*) from BOOKLIST where ISBN = ?", (isbn,))
        if cur.fetchone()[0] > 0:
            PrintErrorPage("ISBN が重複しています．")
            return

        next_id = GetNextId(cur)
        cur.execute(
            "insert into BOOKLIST (ID, AUTHOR, TITLE, PUBLISHER, ISBN) values (?, ?, ?, ?, ?)",
            (next_id, author, title, publisher, isbn),
        )
        con.commit()
    except sqlite3.Error as e:
        PrintErrorPage(f"Error occurred: {e.args[0]}")
        return
    finally:
        con.close()

    print("Content-Type: text/html; charset=utf-8\n")
    print("""<html>
  <head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>追加完了</title>
    <style>
      body {
        font-family: sans-serif;
        text-align: center;
      }
      .nav {
        position: absolute;
        top: 12px;
        left: 12px;
      }
    </style>
  </head>
  <body>
    <div class="nav">
      <a href="/searchform.html">ホーム画面に戻る</a>
    </div>
    <h1>書籍を追加しました</h1>
  </body>
</html>
""")

if __name__ == "__main__":
    main()
