The Emulator Handbook

La documentazione che segue è suddivisa in 3 parti:

- utilizzo dell'emulatore come sola macchina virtuale.
- utilizzo dell'emulatore con compilatore.
- Utilizzo dell'emulatore con interfaccia grafica (form di windows)

con aggiunta di qualche esempio su come programmare in pdp8 (Macchina Didattica del corso)

----------------------------------------------------------------------------------------------------------------------------

Per prima cosa, si noti che l'emulatore è costruito solamente per sistema operativo Windows ed è quindi impossibile l'utilizzo in sistemi operativi Linux.

è sufficiente aprire il file eseguibile per avviare il programma.

----------------------------------------------------------------------------------------------------------------------------

Utilizzo dell'emulatore come virtual machine:

è possibile programmare nel linguaggio di pdp8 senza l'utilizzo del compilatore e dell'interfaccia grafica, questo necessita però di modificare direttamente il codice del programma.

Per prima cosa si apra il file "pdp8.cs" ed individuare la funzione di main. 

a questo punto commentare tutto e instanziare, sempre all'interno del main, una nuova macchina virtuale, chiamando successivamente la funzione di run() seguita dalle funzioni di dump della memoria e di stallo del terminale che altrimenti si chiuderebbe automaticamente alla fine del processo. (windows)
come di seguito:

    pdp8 vm = new pdp8();

    vm.run();
    Printf.printRegisters(vm);
    Printf.printSpecRam(0, 11, vm.ram);
    Console.WriteLine("Press any key to close the application");
    Console.ReadKey();

a questo punto è possibile programmare in pdp8 assegnando ad ogni registro di ram della vm la sua instruzione tramite l'apposita funzione di build, scrivendo però tra l'instanza della vm e la funzione di run, 
come ad esempio:

    pdp8 vm = new pdp8();

        vm.ram[0] = instructionAssembler.buildMRIop(MRI.LDA, new u12(11));
        vm.ram[1] = instructionAssembler.buildMRIop(MRI.ADD, new u12(10));
        vm.ram[2] = instructionAssembler.buildRRIop(RRI.CMA);
        vm.ram[3] = instructionAssembler.buildRRIop(RRI.INC);
        vm.ram[4] = instructionAssembler.buildMRIop(MRI.STA, new u12(11));
        vm.ram[5] = instructionAssembler.buildRRIop(RRI.HLT);
        vm.ram[10].setValue(7);
        vm.ram[11].setValue(15);

    vm.run();
    Printf.printRegisters(vm);
    Printf.printSpecRam(0, 11, vm.ram);
    Console.WriteLine("Press any key to close the application");
    Console.ReadKey();

il codice appena descritto carica nell'accumulatore il valore nel registro 11, somma poi l'accumulatore con il valore contenuto nel registro 10, per poi complementarlo ad uno ed incrementarlo di uno per avere il complemnto a due dell'accumulatore. Infine salva l'accumulatore sul registro 11 e la macchina si spenge (HLT).
le ultime due righe sono variabili.

quanto segue il print su terminale a fine esecuzione:

    Fetch
    found MRI ops
    execute LDA
    MAR <- MBR(AD)
    MBR <- M, AC <- 0
    AC <- AC + MBR

    Fetch
    found MRI ops
    execute ADD
    MAR <- MBR(AD)
    MBR <- M
    EAC <- AC + MBR

    Fetch
    found RRI ops
    execute CMA
    AC <- AC'

    Fetch
    found RRI ops
    execute INC
    E-AC <- AC + 1

    Fetch
    found MRI ops
    execute STA
    MAR <- MBR(AD)
    MBR <- AC
    M <- MBR

    Fetch
    found RRI ops
    execute HLT
    S <- 0

    MBR: DEC 28673 BIN 0111000000000001
    A: DEC -22 BIN 1111111111101010
    MAR: DEC 5 BIN 000000000101
    PC: DEC 6 BIN 000000000110
    S: 0 F: 1 R: 0 I: 0 E: 0
    OPR: 111
    register: 0 DEC: 8203 BIN: 0010000000001011
    register: 1 DEC: 4106 BIN: 0001000000001010
    register: 2 DEC: 29184 BIN: 0111001000000000
    register: 3 DEC: 28704 BIN: 0111000000100000
    register: 4 DEC: 12299 BIN: 0011000000001011
    register: 5 DEC: 28673 BIN: 0111000000000001
    register: 6 DEC: 0 BIN: 0000000000000000
    Press any key to close the application

Più nel dettaglio, le funzioni per buildare le istruzioni pdp8 si trovano nella classe "instructionAssembler" e vanno quindi chiamate come sopra descritto. (InstructionAssembler."funzione apposita").

ora, le istruzioni del pdp8 si suddividono in 3 categorie: 

- Memory reference instruction    MRI
- Register Reference instruction  RRI
- I/O instruction                 IOI

la vm mantiene questa divisione, quindi ogni tipo di funzione di build è riferita ad una categoria, troviamo quindi 

- instructionAssembler.buildMRIop
- instructionAssembler.buildRRIop
- instructionAssembler.buildIOIop

come nell'esempio, buildMRIop prende come parametri: l'istruzione da buildare, il registro a 12 bit al quale ci si riferisce (in questo caso l'i-esima poizione nell'array della ram, come si vede chiaramente nell'esempio), e, nel caso si voglia aggiungere l'indirizzamento indiretto, basta aggiungere come terzo parametro "addressing.indirect"
così:

        vm.ram[0] = instructionAssembler.buildMRIop(MRI.LDA, new u12(11), addressing.indirect);

le altre funzioni, ovvero:

    buildRRIop()
    buildIOIop()

prendono come parametro la sola istruzione.

tutte le istruzioni vanno passate tramite la loro descrizione come enum, quindi ogni istruzione fa riferimento al suo enum:

    MRI.op
    RRI.op
    IOI.op

in assenza di un compilatore è ovvio che non è possibile creare delle label e quindi è necessario riferirsi agli indirizzi di memoria esplicitamente con il loro indirizzo.

in ultimo, Printf.printSpecRam() è una funzione che permette di dumpare un certo range di memoria come nell'esempio descritto precedentemente, che dumpa la memoria dal banco 0 al banco 11, in alternativa è possibile utilizzare la funzione Printf.printRam(vm.ram[]) che dumpa tutta la memoria (2^12 registri, quindi parecchi).

----------------------------------------------------------------------------------------------------------------------------

continua...





