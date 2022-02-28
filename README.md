per adesso l'unico modo per programmare con la macchina virtuale è utilizzare le funzioni che si trovano sulla classe assemblerInstruction.

utilizzare quindi queste funzioni sul main assegnando ad ogni registro di ram la sua istruzione, ad esempio:

	    vm.ram[0] = instructionAssembler.buildMRIop(MRI.LDA, new u12(11));
            vm.ram[1] = instructionAssembler.buildRRIop(RRI.INC);
            vm.ram[2] = instructionAssembler.buildRRIop(RRI.SNA); 
            vm.ram[3] = instructionAssembler.buildMRIop(MRI.BUN, new u12(0));
            vm.ram[4] = instructionAssembler.buildRRIop(RRI.CME);
            vm.ram[5] = instructionAssembler.buildRRIop(RRI.HLT);
            vm.ram[10].setValue(0);
            vm.ram[11].setValue(short.MaxValue);

la prima riga carica nell'accumulatore la cella con indirizzo 11, (LDA)
successivamente incrementa l'accumulatore di 1 (ac = ac + 1) (INC)
la terza riga skippa la prossima istruzione se l'accumulatore è negativo (SNA), in questo caso quindi avverrà lo skip perché maxvalue + 1, in modulo e segno, torna minvalue
quindi la quarta riga è saltata (BUN, salto incondizionato all'indirizzo indicato (goto???))
la quinta riga complementa l'accumulatore (complemento a uno) (CMA)
la sesta riga spenge la macchina (HLT)

le ultime due righe sono variabili.

le istruzioni sono suddivise in MRI (memory reference instruction), RRI (register reference instruction) e IOI (I/O instruction)

le MRI possono avere indirizzamento indiretto, in questo caso, per usarlo, si aggiunge alla funzoine di buildMRIop come terzo parametro l'enum addressing.indirect: esempio:

	    vm.ram[0] = instructionAssembler.buildMRIop(MRI.LDA, new u12(11), addressing.indirect);

che di default è settato a false, quindi diretto.


