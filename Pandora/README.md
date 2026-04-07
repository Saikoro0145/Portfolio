# Pandoraのボス Poseidon

立命館大学情報理工学部プロジェクト団体 RiG++ の活動で制作した、探索型 2D アドベンチャーゲーム「Pandora」のボス「Poseidon」のコードです。

このリポジトリには、「Pandora」のソースコードのうち私が作成した部分のみを抜粋しているため、依存クラスやアセットが不足している箇所があります。

## 使用技術

- C#
- Unity
    - Unity Animator
    - Rigidbody2D / Collider2D を用いた 2D 物理挙動
    - ステートパターンを用いたボス制御
    - SerializeField と Prefab を用いたパラメータ調整・攻撃生成

## 工夫点

- ボス全体の状態と各行動状態を分けた階層型ステートマシンを実装し、`Born`、`Battle`、`Die` と各攻撃行動を分離して管理しやすくしました。
- `BossContextBase` に Animator やフェーズ閾値などの共通情報を集約し、各ステートが必要な情報へ一貫した形でアクセスできるようにしました。
- `BossActionStateManagerBase` でフェーズごとの行動候補プールを切り替えられるようにし、HP に応じて行動パターンが変化するボス戦を構成しました。
- 弾、泡、レーザー、尻尾攻撃を個別クラスに分け、攻撃ロジックと本体の状態遷移を疎結合にすることで、調整と再利用をしやすくしました。
- 多くの調整値を Inspector から変更できるようにし、実装を触らなくても移動速度、待機時間、攻撃間隔などをチューニングできるようにしました。

## 試遊映像

青い蛇のような敵が、私がコーディングを担当した「Poseidon」です。

[試遊映像](https://drive.google.com/file/d/1T4Tf1zb8p3K6DbXwrwSmm6l106GGGkQM/view?usp=drive_link)
