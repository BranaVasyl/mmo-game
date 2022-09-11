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

            if (states.inventoryManager.leftHandData != null && states.inventoryManager.rightHandData != null)
            {
                UpdateActionsWithLeftHand();
                return;
            }

            bool mirror = false;
            ItemWeaponData w = states.inventoryManager.rightHandData;
            if (w == null)
            {
                w = states.inventoryManager.leftHandData;
                mirror = true;
            }

            if (w == null)
            {
                return;
            }

            for (int i = 0; i < w.actions.Count; i++)
            {
                Action a = GetAction(w.actions[i].input);
                a.mirror = mirror;
                a.type = w.actions[i].type;

                DeepCopyStepsList(w.actions[i], a);
            }
        }

        public void UpdateActionsWithLeftHand()
        {
            ItemWeaponData r_w = states.inventoryManager.rightHandData;
            ItemWeaponData l_w = states.inventoryManager.leftHandData;

            if (r_w != null)
            {
                Action rb = GetAction(ActionInput.rb);
                Action rt = GetAction(ActionInput.rt);

                Action w_rb = r_w.GetAction(r_w.actions, ActionInput.rb);
                rb.type = w_rb.type;
                rb.targetAnim = w_rb.targetAnim;
                DeepCopyStepsList(w_rb, rb);

                Action w_rt = r_w.GetAction(r_w.actions, ActionInput.rt);
                rt.type = w_rt.type;
                rt.targetAnim = w_rt.targetAnim;
                DeepCopyStepsList(w_rt, rt);
            }

            if (l_w != null)
            {
                Action lb = GetAction(ActionInput.lb);
                Action lt = GetAction(ActionInput.lt);

                Action w_lb = l_w.GetAction(l_w.actions, ActionInput.rb);
                lb.type = w_lb.type;
                lb.targetAnim = w_lb.targetAnim;
                DeepCopyStepsList(w_lb, lb);

                Action w_lt = l_w.GetAction(l_w.actions, ActionInput.rt);
                lt.type = w_lt.type;
                lt.targetAnim = w_lt.targetAnim;
                DeepCopyStepsList(w_lt, lt);

                if (l_w.LeftHandMirror)
                {
                    lb.mirror = true;
                    lt.mirror = true;
                }
            }
        }

        public static void DeepCopyStepsList(Action from, Action to)
        {
            to.steps = new List<ActionSteps>();

            for (int i = 0; i < from.steps.Count; i++)
            {
                ActionSteps step = new ActionSteps();
                DeepCopyStep(from.steps[i], step);
                to.steps.Add(step);
            }
        }

        public static void DeepCopyStep(ActionSteps from, ActionSteps to)
        {
            to.branches = new List<ActionAnim>();

            for (int i = 0; i < from.branches.Count; i++)
            {
                ActionAnim a = new ActionAnim();
                a.input = from.branches[i].input;
                a.targetAnim = from.branches[i].targetAnim;
                to.branches.Add(a);
            }
        }

        public void UpdateActionsTwoHanded()
        {
            EmptyAllSlots();
            ItemWeaponData w = states.inventoryManager.rightHandData;
            if (w == null)
            {
                return;
            }

            for (int i = 0; i < w.two_handedActions.Count; i++)
            {
                Action a = GetAction(w.two_handedActions[i].input);
                a.steps = w.two_handedActions[i].steps;
                a.type = w.two_handedActions[i].type;
                a.targetAnim = w.two_handedActions[i].targetAnim;
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
            }
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
    }


    public enum ActionInput
    {
        rb, lb, rt, lt
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
        public string targetAnim;
        public List<ActionSteps> steps;

        public bool mirror = false;

        ActionSteps defaultStep;

        public ActionSteps GetActionStep(ref int indx)
        {
            if (steps == null || steps.Count == 0)
            {
                if (defaultStep == null)
                {
                    defaultStep = new ActionSteps();
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
