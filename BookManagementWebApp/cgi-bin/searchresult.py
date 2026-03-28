#!/usr/bin/env python3
import sys
import io
import os
import urllib.parse
import sqlite3
import unicodedata
import html
DB_PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "bookdb.db"))

def main():
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    query = GetParamStr()
    keywords = NormalizeKeywords(query)
    result = SearchInDB(keywords)
    PrintHtmlPage(query, result)

def GetParamStr():
    content_length = os.environ.get('CONTENT_LENGTH') # 入力データ長を取得
    if not content_length:
        return ''
    body = sys.stdin.read(int(content_length)) # 入力データを標準入力から読み込み
    params = urllib.parse.parse_qs(body, keep_blank_values=True)
    return params.get('param1', [''])[0] # ブラウザから送信されたparam1の値を取得

def NormalizeKeywords(query):
    normalized = unicodedata.normalize('NFKC', query).casefold()
    keywords = normalized.split()
    if not keywords:
        return ['']
    return keywords

def NormalizeText(value):
    return unicodedata.normalize('NFKC', value).casefold()

def SearchInDB(keywords):
    con = sqlite3.connect(DB_PATH)	# データベースに接続
    con.row_factory = sqlite3.Row	  # 属性名で値を取り出せるようにする
    cur = con.cursor()				      # カーソルを取得

    try:
        cur.execute("select * from BOOKLIST")
        rows = cur.fetchall()		    # 検索結果をリストとして取得
        if keywords == ['']:
            return rows

        matched = []
        for row in rows:
            title = NormalizeText(row['TITLE'] or '')
            author = NormalizeText(row['AUTHOR'] or '')
            if all((kw in title) or (kw in author) for kw in keywords):
                matched.append(row)
        rows = matched

    except sqlite3.Error as e:		  # エラー処理
        print("Error occurred:", e.args[0])

    con.commit()
    con.close()

    return rows

def EscapeValue(value):
    return html.escape(str(value), quote=True)

def PrintHtmlPage(query, rows):
    display_query = html.escape(query)
    value_query = html.escape(query, quote=True)
    print("Content-Type: text/html; charset=utf-8\n")
    print(f"""
<html>
  <head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>検索結果</title>
    <style>
      body {{
        margin: 0;
        padding: 24px 16px 40px;
        font-family: "Yu Gothic", "Hiragino Kaku Gothic ProN", "Meiryo", sans-serif;
        font-size: 16px;
        color: #222;
        background: #f6f6f8;
      }}
      .nav {{
        margin-bottom: 12px;
      }}
      .nav a {{
        display: inline-block;
        padding: 6px 12px;
        border: 1px solid #888;
        border-radius: 4px;
        background: #f0f0f0;
        color: #222;
        text-decoration: none;
        font-size: 14px;
      }}
      .nav a:hover {{
        background: #e6e6e6;
      }}
      .container {{
        width: 92%;
        max-width: 960px;
        margin: 0 auto;
      }}
      h1 {{
        text-align: center;
        margin: 0 0 18px;
        font-size: 24px;
      }}
      table {{
        border-collapse: collapse;
        margin: 0 auto;
        width: 80%;
      }}
      th, td {{
        border: 1px solid #999;
        padding: 4px 8px;
        text-align: left;
      }}
      th {{
        white-space: nowrap;
      }}
      th {{
        background-color: #6699cc;
        color: #ffffff;
      }}
      thead tr:first-child th {{
        background-color: #eeeeee;
        color: #000000;
        text-align: center;
        font-size: 1.2em;
      }}
      .empty {{
        padding: 16px;
        background: #fff;
        border: 1px solid #ddd;
        border-radius: 10px;
        text-align: center;
      }}
    </style>
  </head>
  <body>
    <div class="container">
      <div class="nav">
        <a href="/searchform.html">ホーム画面に戻る</a>
      </div>
      <h1>"{display_query}" の検索結果一覧</h1>
""")

    if not rows:
        print(f"「{display_query}」をタイトルまたは著者に含む本はありません．")
        print("""    </div>
  </body>
</html>
""")
        return

    print(f"""
      <table>
        <thead>
          <tr>
            <th>タイトル</th>
            <th>著者</th>
            <th>出版社</th>
            <th>ISBN</th>
            <th>削除</th>
          </tr>
        </thead>
        <tbody>
""")

    for row in rows:
        print(f"""        <tr>
          <td>{EscapeValue(row['TITLE'])}</td>
          <td>{EscapeValue(row['AUTHOR'])}</td>
          <td>{EscapeValue(row['PUBLISHER'])}</td>
          <td>{EscapeValue(row['ISBN'])}</td>
          <td>
            <form method="POST" action="/cgi-bin/deletebook.py" onsubmit="return confirm('削除しますか？')">
              <input type="hidden" name="id" value="{EscapeValue(row['ID'])}" />
              <input type="hidden" name="param1" value="{value_query}" />
              <button type="submit">削除</button>
            </form>
          </td>
        </tr>""")

    print("""
        </tbody>
      </table>
    </div>
  </body>
</html>
""")

if __name__ == "__main__":
    main()
