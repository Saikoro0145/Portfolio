using System.Collections.Generic;

//何らかの状態を子に持つための抽象クラス，BossSMにも必要．クラス名良いのがあれば変更したい．
public abstract class BossStateAsParent
{
    protected internal List<BossStateBase> children = new(); //internal付けないとエラーのため付与．不要と考えるが...
    public abstract void Change(BossStateBase child);
}