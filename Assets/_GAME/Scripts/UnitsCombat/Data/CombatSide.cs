// Which team a combat event belongs to. Targeting already decides friend from
// foe through its own providers; this exists purely so feedback can tell the
// two apart - an enemy kill is a reward and gets the punch, a soldier going
// down is a loss and must not.
public enum CombatSide
{
    Ally = 0,
    Enemy = 1,
}
