using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum WorkerTask {Checkout, Restocking}

public class Worker : MonoBehaviour
{
    [SerializeField] private Transform productsParent;
    private CashRegister cashRegister;
    private Container container;
    private ShelfTrigger shelfTrigger;
    private NavMeshAgent agent;
    private Animator animator;

    private WorkerTask workerTask = WorkerTask.Checkout;
    private string workerName = "John Smith";
    private int workerWage = 10;

    private bool isWalking = false;
    private bool isPerformingNotStoppableAction = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponentInChildren<Animator>();
        StartCoroutine(SelectAction());
        DOTween.Init();
    }

    public void ChangeWorkerAction(WorkerTask workerTask)
    {
        if (this.workerTask == workerTask)
            return;
        this.workerTask = workerTask;
        if (isWalking || isPerformingNotStoppableAction)
            return;
        StopAction();
        StartCoroutine(SelectAction());
    }

    private void StopAction()
    {
        if (cashRegister != null) {
            cashRegister.RemoveWorker();
            cashRegister.OnDestroy -= OnCashRegisterDestroy;
            cashRegister = null;
        }
        if(container != null) {
            container.Place(transform.position, transform.rotation, null);
            container = null;
        }
    }

    public void Init(string name, int workerTask, int wage)
    {
        this.workerName = name;
        this.workerWage = wage;
        this.workerTask = (WorkerTask)workerTask;
    }

    public void GetWorkerData(out string name, out WorkerTask workerTask, out int wage)
    {
        name = this.workerName;
        workerTask = this.workerTask;
        wage = this.workerWage;
    }

    IEnumerator SelectAction()
    {
        yield return new WaitForSeconds(.75f);
        StopAction();
        yield return new WaitForSeconds(.75f);

        switch (workerTask) {
            case WorkerTask.Checkout:
                StartCoroutine(SelectActionCheckout());
                break;
            case WorkerTask.Restocking:
                StartCoroutine(SelectActionRestocking());
                break;
            default:
                break;        
        }
        yield break;
    }

    IEnumerator SelectActionCheckout()
    {
        cashRegister = GetClosestCashRegister();
        if (cashRegister == null) {
            StartCoroutine(SelectAction());
            yield break;
        }

        yield return StartCoroutine(GoTo(cashRegister.workerDestination.position));

        if (cashRegister == null || cashRegister.isWorker || workerTask != WorkerTask.Checkout) {
            cashRegister = null;
            StartCoroutine(SelectAction());
            yield break;
        }
        cashRegister.AddWorker();
        cashRegister.OnDestroy += OnCashRegisterDestroy;
        transform.DORotate(-cashRegister.transform.eulerAngles, 0.75f);
    }

    IEnumerator SelectActionRestocking()
    {
        Container containerToTake = ProductsData.instance.GetContainerForRestocker();

        bool ShouldStop(){
            return shelfTrigger == null || containerToTake == null || workerTask != WorkerTask.Restocking;
        }
        
        if (containerToTake == null) {
            yield return new WaitForSeconds(1f);
            StartCoroutine(SelectAction());
            yield break;
        }
        Vector3 destination = containerToTake.containerGO.transform.position;
        destination.y = 0;
        yield return StartCoroutine(GoTo(destination));
        destination = containerToTake.containerGO.transform.position;
        destination.y = 0;
        shelfTrigger = ShopData.instance.GetEmptyShelfTrigger();
        if (ShouldStop() || containerToTake.containerGO == null || Vector3.Distance(transform.position, destination) > 0.5f) {
            containerToTake = null;
            StartCoroutine(SelectAction());
            yield break;
        }
        container = containerToTake;
        containerToTake.OnPlayerTake(true, productsParent);
        destination = shelfTrigger.shelf.customerDestination.position;
        yield return StartCoroutine(GoTo(destination));
        destination = shelfTrigger.shelf.customerDestination.position;
        if (ShouldStop() || Vector3.Distance(transform.position, destination) > 0.5f) {
            containerToTake.Place(transform.position, transform.rotation, null);
            containerToTake = null;
            StartCoroutine(SelectAction());
            yield break;
        }
        
        container.GetProductsInContainerData(out List<Product> productsInContainer, out List<Vector3> _, out List<Vector3> _);
        ProductsData.instance.GetInTriggerPositions(productsInContainer, shelfTrigger.trigger, out List<Vector3> positions, false);
        Vector3 basePosition = shelfTrigger.transform.position + Quaternion.Euler(0, 90, 0) * (shelfTrigger.trigger.center - shelfTrigger.trigger.size * 0.5f);
        for (int i = 0; i < positions.Count; i++) {            
            Quaternion rotation = shelfTrigger.trigger.transform.rotation;
            productsInContainer[i].Place(positions[i] + basePosition, rotation, null);
            productsInContainer[i].productGO.transform.RotateAround(basePosition, Vector3.up, 90);
        }
        container.ClearProductsInContainer();
        StartCoroutine(SelectAction());
        yield break;
    }

    private CashRegister GetClosestCashRegister()
    {
        List<CashRegister> registers = ShopData.instance.registersList;
        CashRegister selectedRegister = null;

        foreach (CashRegister register in registers) {
            if (!register.isWorker) {
                if(selectedRegister == null) {
                    selectedRegister = register;
                    continue;
                }
                if(Vector3.Distance(selectedRegister.transform.position, transform.position) > Vector3.Distance(register.transform.position, transform.position)) {
                    selectedRegister = register;
                }
            }
        }
        return selectedRegister;
    }

    public IEnumerator GoTo(Vector3 destination)
    {
        isWalking = true;
        agent.SetDestination(destination);
        animator.SetBool("isWalking", true);
        while (Vector3.Distance(transform.position, destination) > 0.5f) {
            yield return new WaitForSeconds(0.2f);
        }
        isWalking = false;
        animator.SetBool("isWalking", false);
    }

    private void OnCashRegisterDestroy()
    {
        StartCoroutine(SelectAction());
    }


    private void OnDestroy()
    {
        if (cashRegister != null) {
            cashRegister.RemoveWorker();
            cashRegister.OnDestroy -= OnCashRegisterDestroy;
        }
    }
}
