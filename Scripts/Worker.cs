using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum WorkerTask { Checkout, Restocking, Cleaning, Fired}

public class Worker : MonoBehaviour, IInformationDisplay
{
    [SerializeField] private Transform productsParent;
    [SerializeField] private float placingProductsCooldown;
    private CashRegister cashRegister;
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private Transform Hand;
    [SerializeField] GameObject Mop;

    private WorkerTask workerTask = WorkerTask.Checkout;
    private string workerName = "John Smith";
    [HideInInspector]
    public int workerWage = 10;
    private bool isFemale;

    private const float defaultDistanceAllowed = 0.5f;

    private delegate bool StopDelegate();
    private ContainerGO takenContainer;

    [SerializeField] private Transform Model;
    [SerializeField] private Vector3 DrivingOffsetPos;
    [SerializeField] private Quaternion DrivingOffsetRot;
    private CleaningMachine currentCleaningMachine;
    private float originalSpeed;

    public string InformationDisplayText => workerName + " (" + workerTask.ToString() + ")";
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponentInChildren<Animator>();
        StartCoroutine(SelectAction());
        DOTween.Init();
    }


    public void Init(string name, int workerTask, int wage, bool isFemale, int containerIndex)
    {
        this.workerName = name;
        this.workerWage = wage;
        this.isFemale = isFemale;
        this.workerTask = (WorkerTask)workerTask;
        if(containerIndex != -1) {
            ProductsData.instance.containersSpawned[containerIndex].Place(transform.position, Quaternion.identity, null);
        }
    }

    public void GetWorkerData(out string workerName, out WorkerTask workerTask, out int wage)
    {
        workerName = this.workerName;
        workerTask = this.workerTask;
        wage = this.workerWage;
    }

    public WorkerSaveData GetWorkerSaveData()
    {
        int pickableID = -1;
        if(takenContainer != null) {
            pickableID = takenContainer.container.PickableID;
        }
        return new WorkerSaveData(transform.position, workerName, (int)workerTask, workerWage, isFemale, pickableID);
    }

    public void ChangeWorkerTask(WorkerTask workerTask)
    {
        this.workerTask = workerTask;
    }

    IEnumerator SelectAction()
    {
        //Debug.Log("Selecting worker action");
        yield return new WaitForSeconds(.75f);
        yield return new WaitForSeconds(.75f);

        switch (workerTask)
        {
            case WorkerTask.Checkout:
                yield return StartCoroutine(ActionCheckout());
                break;
            case WorkerTask.Restocking:
                yield return StartCoroutine(ActionRestockingStart());
                break;
            case WorkerTask.Cleaning:
                yield return StartCoroutine(ActionCleaning());
                break;
            case WorkerTask.Fired:
                Destroy(gameObject);
                yield break;
            default:
                break;
        }
        StartCoroutine(SelectAction());
        yield break;
    }

    IEnumerator ActionCleaning()
    {
        currentCleaningMachine = null;

        while (workerTask == WorkerTask.Cleaning)
        {
            if(currentCleaningMachine == null)
            {
                foreach (CleaningMachine machine in WorkersManager.instance.CleaningMachines)
                {
                    if (!machine.isClaimed)
                    {
                        currentCleaningMachine = machine;
                        break;
                    }
                }

                if (currentCleaningMachine != null)
                {
                    currentCleaningMachine.Claim();

                    // Move worker to the machine
                    yield return StartCoroutine(GoTo(currentCleaningMachine.transform, () =>
                        workerTask != WorkerTask.Cleaning || currentCleaningMachine == null));

                    if (workerTask != WorkerTask.Cleaning || currentCleaningMachine == null)
                    {
                        currentCleaningMachine.Release();
                        currentCleaningMachine = null;
                        Mop.SetActive(false);
                        Model.transform.localPosition = Vector3.zero;
                        Model.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        animator.SetBool("isDriving", false);
                        yield break;
                    }

                    currentCleaningMachine.Follow(transform);

                    animator.SetBool("isDriving", true);
                    originalSpeed = agent.speed;
                    agent.speed *= currentCleaningMachine.speedBoost;
                    Model.transform.localPosition = DrivingOffsetPos;
                    Model.transform.localRotation = DrivingOffsetRot;
                    Mop.SetActive(false);
                }
                else
                {
                    Mop.SetActive(true);
                }
            }
            GameObject garbage = GetClosestGarbage();
            if (garbage == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            bool ShouldStop() => garbage == null || workerTask != WorkerTask.Cleaning;

            yield return StartCoroutine(GoTo(garbage.transform, ShouldStop));

            if (ShouldStop())
                break;

            if(currentCleaningMachine == null)
            {
                animator.SetBool("isCleaning", true);
            }
            yield return new WaitForSeconds(1.6f);
            CustomerManager.instance.SpawnedGarbage.Remove(garbage);
            Destroy(garbage);
            yield return new WaitForSeconds(1f);
            animator.SetBool("isCleaning", false);
        }

        if (currentCleaningMachine != null)
        {
            currentCleaningMachine.Release();
            agent.speed = originalSpeed;
            animator.SetBool("isDriving", false);
            currentCleaningMachine = null;
        }
        Mop.SetActive(false);
    }

    GameObject GetClosestGarbage()
    {
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 workerPosition = transform.position;

        foreach (GameObject garbage in CustomerManager.instance.SpawnedGarbage)
        {
            Garbage garbageScript = garbage.GetComponent<Garbage>();
            if (garbage == null || garbageScript.isTaken) continue;

            float distance = Vector3.Distance(workerPosition, garbage.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                garbageScript.isTaken = true;
                if(closest != null)
                {
                    closest.GetComponent<Garbage>().isTaken = true;
                }
                closest = garbage;
            }
        }

        return closest;
    }


    IEnumerator ActionCheckout()
    {
        Mop.SetActive(false);
        Model.transform.localPosition = Vector3.zero;
        Model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        animator.SetBool("isDriving", false);

        bool ShouldStop(){
            return cashRegister == null || cashRegister.isWorker || workerTask != WorkerTask.Checkout;
        }

        cashRegister = GetClosestCashRegister();
        if (cashRegister == null) {
            yield break;
        }

        yield return StartCoroutine(GoTo(cashRegister.workerDestination, ShouldStop));

        if (ShouldStop()) {
            cashRegister = null;
            yield break;
        }
        cashRegister.AddWorker();
        transform.DORotate(cashRegister.transform.eulerAngles + new Vector3(0, 180, 0), 0.75f);

        while (workerTask == WorkerTask.Checkout && cashRegister != null){
            yield return new WaitForSeconds(1);
        }
        if (cashRegister != null) {
            cashRegister.RemoveWorker();
            cashRegister = null;
        }
        yield break;
    }

    IEnumerator ActionRestockingStart()
    {
        Mop.SetActive(false);
        Model.transform.localPosition = Vector3.zero;
        Model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        animator.SetBool("isDriving", false);
        yield return new WaitForSeconds(0.1f);
        WorkersManager.instance.GetActionForRestocker(transform.position, out Container containerToHandle, out PlacingTriggerArea shelfTrigger, out bool placeOnlyContainer);
        yield return StartCoroutine(ActionRestocking(containerToHandle, shelfTrigger, placeOnlyContainer));
    }
    IEnumerator ActionRestocking(Container containerToHandle, PlacingTriggerArea shelfTrigger, bool placeOnlyContainer)
    {
        //Debug.Log("Action restocking");
        //Debug.Log(containerToHandle + " " + shelfTrigger + " " + placeOnlyContainer);
        WorkersManager.instance.BlockUsage(containerToHandle, shelfTrigger);
        if (containerToHandle != null) {
            if (containerToHandle.IsContainerEmpty() && GetClosestDumpster() != null)
                yield return StartCoroutine(ActionThrowAway(containerToHandle));
            else {
                if (shelfTrigger != null) {
                    yield return StartCoroutine(ActionPlaceOnShelf(containerToHandle, shelfTrigger, placeOnlyContainer));
                }
            }
        }
        yield return new WaitForEndOfFrame();
        if (takenContainer != null) {
            //takenContainer.container.Place(transform.position, transform.rotation, null);
            //PlaceContainer(transform.position, transform.rotation);
            if (containerToHandle.IsContainerEmpty()) {
                if (GetClosestDumpster() != null)
                    yield return StartCoroutine(ActionThrowAway(containerToHandle));
            }
            else {
                WorkersManager.instance.UnblockUsage(null, shelfTrigger);
                shelfTrigger = null;
                WorkersManager.instance.GetActionForRestocker(transform.position, containerToHandle, out PlacingTriggerArea nextShelfTrigger, out placeOnlyContainer);
                if (nextShelfTrigger != null && workerTask == WorkerTask.Restocking) {
                    yield return StartCoroutine(ActionRestocking(containerToHandle, nextShelfTrigger, placeOnlyContainer));
                }
            }            
        }
        if (takenContainer != null)
            PlaceContainer(transform.position, transform.rotation);

        WorkersManager.instance.UnblockUsage(containerToHandle, shelfTrigger);
        yield break;
    }

    IEnumerator ActionTakeContainer(ContainerGO containerGO)
    {
        bool ShouldStopAction()
        {
            return containerGO == null || workerTask != WorkerTask.Restocking;
        }
        Container container = containerGO.container;
        yield return StartCoroutine(GoTo(container.GetTakingPosition(), ShouldStopAction));
        Vector3 destination = container.GetTakingPosition().position;
        destination.y = 24;
        if (ShouldStopAction() || Vector3.Distance(transform.position, destination) > defaultDistanceAllowed) {
            yield break;
        }
        container.OnPlayerTake(true, productsParent);
        takenContainer = container.containerGO;

        // SCALE TAKEN BOX TO FIT WORKER HANDS
        Vector3 baseScale = new Vector3(0.55f, 0.4f, 0.60f);
        Vector3 colliderSize = Vector3.Scale(container.BoxCollider.size, container.BoxCollider.transform.localScale);
        float maxScale = colliderSize.x;
        if(maxScale < colliderSize.z) {
            takenContainer.transform.localEulerAngles = new Vector3(0,90,0);
            maxScale = colliderSize.z;
        }
        takenContainer.transform.localScale = Vector3.one * baseScale.z / maxScale;
        //takenContainer.transform.localPosition -= new Vector3(0, (takenContainer.transform.localScale.y - takenContainer.transform.localScale.y) / 2, 0);
        animator.SetLayerWeight(1, 1);
    }

    private void PlaceContainer(Vector3 position, Quaternion rotation)
    {
        takenContainer.container.Place(position, rotation, null);
        animator.SetLayerWeight(1, 0);
    }

    IEnumerator ActionPlaceOnShelf(Container containerToTake, PlacingTriggerArea shelfTrigger, bool placeOnlyContainer)
    {
        bool ShouldStopAction(){
            return shelfTrigger == null || containerToTake == null || workerTask != WorkerTask.Restocking;
        }

        yield return StartCoroutine(ActionTakeContainer(containerToTake.containerGO));
        
        if (ShouldStopAction() || takenContainer == null) {
            yield break;
        }
        //Debug.Log("Action go to shelf");
        yield return StartCoroutine(GoTo(shelfTrigger.InteractionPosition, ShouldStopAction));

        //Debug.Log(ShouldStopAction() + " " + (Vector3.Distance(transform.position, FixedInteractionPosition) > 0.5f));
        if (ShouldStopAction() ) {
            yield break;
        }

        Vector3 FixedInteractionPosition = shelfTrigger.InteractionPosition.position;
        FixedInteractionPosition.y = 24;

        if (Vector3.Distance(transform.position, FixedInteractionPosition) > 0.5f)
        {
            yield break;
        }

        if (placeOnlyContainer) {
            //Debug.Log("Can place: " + shelfTrigger.CanRestockerPlaceContainer(containerToTake));
            if (shelfTrigger.CanRestockerPlaceContainer(containerToTake)) {
                shelfTrigger.PlaceRestockerContainer(containerToTake);
                animator.SetLayerWeight(1, 0);
            }
        }
        else {
            //Debug.Log("Action place products on shelf");
            yield return StartCoroutine(PlaceProductsOnShelf(containerToTake, shelfTrigger));
            if (containerToTake.IsContainerEmpty()) {
                yield return StartCoroutine(ActionThrowAway(containerToTake));
            }
        }        
    }
    private IEnumerator ActionThrowAway(Container containerToThrowAway)
    {
        Dumpster dumpster = GetClosestDumpster();
        bool ShouldStopAction()
        {
            return dumpster == null || containerToThrowAway == null || workerTask != WorkerTask.Restocking;
        }

        yield return StartCoroutine(ActionTakeContainer(containerToThrowAway.containerGO));
        if (ShouldStopAction() || takenContainer == null) {
            yield break;
        }

        yield return StartCoroutine(GoTo(dumpster.workersDestination, ShouldStopAction));
        if (ShouldStopAction() || Vector3.Distance(transform.position, dumpster.workersDestination.position) > 0.5f) {
            //PlaceContainer(transform.position, transform.rotation);
            //containerToThrowAway.Place(transform.position, transform.rotation, null);
            yield break;
        }
        containerToThrowAway.RemoveFromGame(true);
        animator.SetLayerWeight(1, 0);
        AudioManager.PlaySound(Sound.ThrowAway, dumpster.transform.position);
        TasksManager.instance.ProgressTasks(TaskType.ThrowAwayBoxes, 1);
    }

    private IEnumerator PlaceProductsOnShelf(Container container, PlacingTriggerArea shelfTrigger)
    {
        ProductSO placingProductType = shelfTrigger.currentProduct;
        container.GetProductsInContainerData(out List<Product> productsInContainer, out List<Vector3> _, out List<Vector3> _);
        List<Product> productsToPlaceNext = new List<Product>();
        shelfTrigger.GetProductsPlacingPositions(out List<Vector3> placingPositions, out List<Quaternion> placingRotation);
        Vector3 productColliderSize = placingProductType.prefab.GetComponentInChildren<BoxCollider>().size;

        for (int i = 0; i < placingPositions.Count; i++) {
            //Debug.Log(productsInContainer.Count);
            if (shelfTrigger == null || shelfTrigger.currentProduct != placingProductType || productsInContainer.Count == 0)
                break;
            Vector3 position = placingPositions[i];
            Quaternion rotation = placingRotation[i];
            Vector3 collisionCheckCenter = position + rotation * new Vector3(0, productColliderSize.y * 0.5f, 0);
            Vector3 halfExtents = productColliderSize / 2f;  // new Vector3(scale.x * size.x, scale.y * size.y, scale.z * size.z) / 2f;
            Collider[] hitColliders = Physics.OverlapBox(collisionCheckCenter, halfExtents, rotation, WorkersManager.instance.pickableLayerMask);
            bool skipThisPosition = false;
            //Debug.Log(hitColliders.Length);
            for (int j = 0; j < hitColliders.Length; j++) {
                ProductGO hitProductGO = hitColliders[j].gameObject.GetComponent<ProductGO>();
                if(hitProductGO == null) {
                    skipThisPosition = true;
                    break;
                }
                if (hitProductGO.product.productType != placingProductType) {
                    skipThisPosition = true;
                    continue;
                }
                if(hitColliders.Length == 1 && Vector3.Distance(hitProductGO.transform.position, position) < .01f) {
                    hitProductGO.transform.position = position;
                    hitProductGO.transform.rotation = rotation;
                    skipThisPosition = true;
                    break;
                }
                hitProductGO.product.OnPlayerTake(false, productsParent);
                productsToPlaceNext.Add(hitProductGO.product);
            }
            //Debug.Log(skipThisPosition);
            if (skipThisPosition)
                continue;
            if (productsToPlaceNext.Count > 0) {
                productsToPlaceNext[0].Place(position, rotation, null, true);
                productsToPlaceNext.RemoveAt(0);
            }
            else {
                productsInContainer[0].Place(position, rotation, null, true);
                container.RemoveProductFromContainer(productsInContainer[0]);
            }                
            yield return new WaitForSeconds(placingProductsCooldown);
        }
        for(int i = 0; i < productsToPlaceNext.Count; i++) {
            Vector3 offset = new Vector3(0, productColliderSize.y * i, 0);
            productsToPlaceNext[i].Place(transform.position + offset, transform.rotation, null, true);
        }
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

    private Dumpster GetClosestDumpster()
    {
        Dumpster selectedDumpster = null;

        foreach (Dumpster dumpster in ShopData.instance.dumpstersList) {            
            if (selectedDumpster == null || Vector3.Distance(selectedDumpster.transform.position, transform.position) > Vector3.Distance(dumpster.transform.position, transform.position)) {
                selectedDumpster = dumpster;
            }            
        }
        return selectedDumpster;
    }

    private IEnumerator GoTo(Transform destination, StopDelegate ShouldStop, float distanceAllowed = defaultDistanceAllowed)
    {
        Vector3 startingPosition = new Vector3(destination.position.x, 24, destination.position.z);
        agent.SetDestination(startingPosition);
        agent.isStopped = false;
        animator.SetBool("isWalking", true);
        int counter = 0;
        while (Vector3.Distance(transform.position, startingPosition) > distanceAllowed && !ShouldStop()) {
            counter++;
            if(counter % 10 == 0) {
                Vector3 currentPosition = new Vector3(destination.position.x, 24, destination.position.z);
                if (Vector3.Distance(startingPosition, currentPosition) > distanceAllowed)
                    break;
            }
            yield return new WaitForSeconds(0.2f);
        }
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
    }
}
