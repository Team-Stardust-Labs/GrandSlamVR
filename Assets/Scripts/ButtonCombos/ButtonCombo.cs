using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonCombo : MonoBehaviour
{

    [SerializeField] protected InputAction triggerA;
    [SerializeField] protected InputAction triggerB;
    [SerializeField] protected InputAction triggerC;
    [SerializeField] protected InputAction triggerD;
    [SerializeField] protected InputAction activationTrigger; // just one more button to better distinguish combos

    private bool lA, lB, rA, rB, aT;

    // to allow it to be called from sublcass it needs to be protected virtual
    protected virtual void OnEnable()
    {
        triggerA.Enable();
        triggerB.Enable();
        triggerC.Enable();
        triggerD.Enable();
        activationTrigger.Enable();

        triggerA.performed += ctx => lA = true;
        triggerA.canceled += ctx => lA = false;

        triggerB.performed += ctx => lB = true;
        triggerB.canceled += ctx => lB = false;

        triggerC.performed += ctx => rA = true;
        triggerC.canceled += ctx => rA = false;

        triggerD.performed += ctx => rB = true;
        triggerD.canceled += ctx => rB = false;

        activationTrigger.performed += ctx => aT = true;
        activationTrigger.canceled += ctx => aT = false;
    }

    private void OnDisable()
    {
        triggerA.Disable();
        triggerB.Disable();
        triggerC.Disable();
        triggerD.Disable();
        activationTrigger.Disable();
    }

    void Update()
    {
        if (lA && lB && rA && rB && aT)
        {
            TriggerEvent(); // Your custom method
        }
    }

    protected virtual void TriggerEvent()
    {

    }

}
