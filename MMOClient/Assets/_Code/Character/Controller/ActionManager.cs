using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class ActionManager : MonoBehaviour
    {
        public int actionIndex;
        public List<Action> actionSlots = new List<Action>();

        public ItemAction consumableItem;

        StateManager states;

        public void Init(StateManager st)
        {
            states = st;
            UpdateActionsOneHanded();
        }

        public void UpdateActionsOneHanded()
        {
            EmptyAllSlots();

            ItemWeaponData rightHandItem = states.inventoryManager.rightHandData;
            if (rightHandItem != null)
            {
                DeepCopyAction(rightHandItem.actions, ActionInput.rb, ActionInput.rb);
                DeepCopyAction(rightHandItem.actions, ActionInput.rt, ActionInput.rt);
            }

            ItemWeaponData leftHandItem = states.inventoryManager.leftHandData;
            if (leftHandItem != null)
            {
                DeepCopyAction(leftHandItem.actions, ActionInput.rb, ActionInput.lb, true);
                DeepCopyAction(leftHandItem.actions, ActionInput.rt, ActionInput.lt, true);
            }
        }

        public void UpdateActionsTwoHanded()
        {
            EmptyAllSlots();

            ItemWeaponData currentWeapon = states.inventoryManager.rightHandData ?? states.inventoryManager.leftHandData;
            if (currentWeapon == null)
            {
                return;
            }

            List<Action> actions = currentWeapon.two_handedActions;
            for (int i = 0; i < actions.Count; i++)
            {
                DeepCopyAction(actions, actions[i].input, actions[i].input);
            }
        }

        public void DeepCopyAction(List<Action> actions, ActionInput inp, ActionInput assign, bool isLeftHand = false)
        {
            Action a = GetAction(assign);
            Action w_a = GetActionFromList(actions, inp);
            if (w_a == null)
            {
                return;
            }

            a.targetAnim = w_a.targetAnim;
            a.type = w_a.type;
            a.spellClass = w_a.spellClass;
            DeepCopyStepsList(w_a, a, isLeftHand);

            if (isLeftHand)
            {
                a.mirror = true;
            }
        }

        public static void DeepCopyStepsList(Action from, Action to, bool isLeft)
        {
            to.steps = new List<ActionSteps>();

            if (from.steps == null)
            {
                return;
            }

            for (int i = 0; i < from.steps.Count; i++)
            {
                ActionSteps step = new ActionSteps();
                DeepCopyStep(from.steps[i], step, isLeft);
                to.steps.Add(step);
            }
        }

        public static void DeepCopyStep(ActionSteps from, ActionSteps to, bool isLeft)
        {
            to.branches = new List<ActionAnim>();

            for (int i = 0; i < from.branches.Count; i++)
            {
                ActionAnim a = new ActionAnim();

                if (!isLeft)
                {
                    a.input = from.branches[i].input;
                }
                else
                {
                    switch (from.branches[i].input)
                    {
                        case ActionInput.rb:
                            a.input = ActionInput.lb;
                            break;
                        case ActionInput.rt:
                            a.input = ActionInput.lt;
                            break;
                        case ActionInput.lb:
                            a.input = ActionInput.rb;
                            break;
                        case ActionInput.lt:
                            a.input = ActionInput.rt;
                            break;
                    }
                }

                a.targetAnim = from.branches[i].targetAnim;
                to.branches.Add(a);
            }
        }

        void EmptyAllSlots()
        {
            for (int i = 0; i < 4; i++)
            {
                Action a = GetAction((ActionInput)i);
                a.steps = null;
                a.mirror = false;
                a.targetAnim = "";
                a.type = ActionType.attack;
                a.defaultStep = null;
            }
        }

        public Action GetActionFromList(List<Action> l, ActionInput inp)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].input == inp)
                {
                    return l[i];
                }
            }

            return null;
        }

        public SpellAction GetSpellActionFromList(List<SpellAction> l, ActionInput inp)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].input == inp)
                {
                    return l[i];
                }
            }

            return null;
        }

        public Action GetActionSlot(StateManager st)
        {
            ActionInput a_input = GetActionInput(st);
            return GetAction(a_input);
        }

        public Action GetActionFromInput(ActionInput a_input)
        {
            return GetAction(a_input);
        }

        Action GetAction(ActionInput inp)
        {
            for (int i = 0; i < actionSlots.Count; i++)
            {
                if (actionSlots[i].input == inp)
                {
                    return actionSlots[i];
                }
            }

            return null;
        }

        public ActionInput GetActionInput(StateManager st)
        {
            if (st.rb)
            {
                return ActionInput.rb;
            }
            if (st.rt)
            {
                return ActionInput.rt;
            }
            if (st.lb)
            {
                return ActionInput.lb;
            }
            if (st.lt)
            {
                return ActionInput.lt;
            }

            return ActionInput.rb;
        }

        ActionManager()
        {
            for (int i = 0; i < 4; i++)
            {
                Action a = new Action();
                a.input = (ActionInput)i;
                actionSlots.Add(a);
            }
        }
    }

    public enum ActionInput
    {
        rb, lb, rt, lt
    }

    public enum SpellClass
    {
        pyromancy, miracles, sorcery
    }

    public enum SpellType
    {
        projectile, buff, looping
    }

    public enum ActionType
    {
        attack, block, spells, parry, interact
    }

    [System.Serializable]
    public class Action
    {
        public ActionInput input;
        public ActionType type;
        public SpellClass spellClass;
        public string targetAnim;
        public List<ActionSteps> steps;

        public bool mirror = false;

        public ActionSteps defaultStep;

        public ActionSteps GetActionStep(ref int indx)
        {
            if (steps == null || steps.Count == 0)
            {
                if (defaultStep == null)
                {
                    defaultStep = new ActionSteps();
                }

                if (defaultStep.branches == null || defaultStep.branches.Count == 0)
                {
                    defaultStep.branches = new List<ActionAnim>();
                    ActionAnim aa = new ActionAnim();
                    aa.input = input;
                    aa.targetAnim = targetAnim;

                    defaultStep.branches.Add(aa);
                }

                return defaultStep;
            }

            if (indx > steps.Count - 1)
            {
                indx = 0;
            }

            ActionSteps retVal = steps[indx];

            if (indx > steps.Count - 1)
            {
                indx = 0;
            }
            else
            {
                indx++;
            }

            return retVal;
        }
    }

    [System.Serializable]
    public class SpellAction
    {
        public ActionInput input;
        public string targetAnim;
        public string throwAnim;
        public float castTime;
    }

    [System.Serializable]
    public class ActionSteps
    {
        public List<ActionAnim> branches = new List<ActionAnim>();

        public ActionAnim GetBranch(ActionInput inp)
        {
            for (int i = 0; i < branches.Count; i++)
            {
                if (branches[i].input == inp)
                {
                    return branches[i];
                }
            }

            return new ActionAnim();
            //return branches[0];
        }
    }

    [System.Serializable]
    public class ActionAnim
    {
        public ActionInput input;
        public string targetAnim;
    }

    [System.Serializable]
    public class ItemAction
    {
        public string targetAnim;
        public string item_id;
    }
}
