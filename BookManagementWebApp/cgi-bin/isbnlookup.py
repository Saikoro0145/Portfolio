#!/usr/bin/env python3
import sys
import io
import os
import json
import urllib.parse
import urllib.request
import urllib.error

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
API_KEY_PATH = os.path.abspath(
    os.path.join(os.path.dirname(__file__), "..", ".env", "GoogleBooksAPIKey.txt")
)

def GetIsbn():
    method = os.environ.get('REQUEST_METHOD', '').upper()
    if method == 'POST':
        content_length = os.environ.get('CONTENT_LENGTH')
        if not content_length:
            return ''
        body = sys.stdin.read(int(content_length))
        params = urllib.parse.parse_qs(body, keep_blank_values=True)
    else:
        params = urllib.parse.parse_qs(os.environ.get('QUERY_STRING', ''), keep_blank_values=True)
    return params.get('isbn', [''])[0]

def LoadGoogleBooksApiKey():
    try:
        with open(API_KEY_PATH, "r", encoding="utf-8") as file:
            return file.read().strip()
    except OSError:
        return ""

def main():
    isbn = GetIsbn().strip()
    print("Content-Type: application/json; charset=utf-8\n")
    if not isbn or not isbn.isdigit() or len(isbn) != 13:
        print("{}")
        return

    params = {"q": "isbn:" + isbn}
    api_key = LoadGoogleBooksApiKey()
    if api_key:
        params["key"] = api_key
    url = "https://www.googleapis.com/books/v1/volumes?" + urllib.parse.urlencode(params)
    try:
        with urllib.request.urlopen(url, timeout=10) as res:
            payload = res.read().decode('utf-8')
        data = json.loads(payload)
    except (urllib.error.URLError, json.JSONDecodeError, UnicodeDecodeError):
        print(json.dumps({"message": "書籍情報の取得に失敗しました"}, ensure_ascii=False))
        return

    items = data.get('items')
    if not items:
        print(json.dumps({"message": "ISBN が一致する書籍情報が見つかりませんでした"}, ensure_ascii=False))
        return

    info = items[0].get('volumeInfo', {})
    title = info.get('title', '')
    authors = info.get('authors') or []
    if isinstance(authors, list):
        author = ", ".join(authors)
    else:
        author = str(authors)
    publisher = info.get('publisher', '')

    result = {"title": title, "author": author, "publisher": publisher}
    print(json.dumps(result, ensure_ascii=False))

if __name__ == "__main__":
    main()
