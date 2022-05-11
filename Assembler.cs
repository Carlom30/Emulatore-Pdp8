using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintSpace;
using UtilityStuff;
using Emulatore_Pdp8;
using System.IO;
using System.Runtime.InteropServices;


namespace Assembler
{
    class CompilerData
    {
        
        public i16[] machineCode;
        public u12 programAddress;
        public LineCode[] tkSource;
        
        public bool completed;

        public CompilerData()
        {
            machineCode = new i16[4096];
            programAddress = new u12(0);
            completed = false;
        }
    }
    struct Token
    {
        public TokenType type;
        public string lexem;
    }
    
    struct LineCode 
    {
        public Token[] pattern;
        public PatternType type;
        public LineCode(Token[] newPattern, PatternType newType)
        {
            pattern = newPattern;
            type = newType;
        }
    }

    enum PatternType
    {
        PT_RRIIOIonly = 0,
        PT_MRILabel = 1,
        PT_MRILabel_Indirect = 2,
        PT_Label_comma_MRILabel = 3,
        PT_Label_comma_MRILabel_Indirect = 4,
        PT_Label_comma_RRIIOI = 5,
        PT_Label_comma_Pseudo_Value = 6,
        PT_Pseudo_value = 7,
        PT_PseudoOnly = 8,
        PT_Pseudo_label = 9, //è probabile che la label sia un valore esadecimale
        PT_Label_comma_Pseudo_Label = 10, //same shit, per non complicare le cose, dopo HEX deve essereci un valore esadecimale, che però il lexer
                                          //scambia per una label, quindi faccio che al lexer va bene di trovare una label, i controlli se effettivamente 
                                          //il valore va bene li farò più avanti nella compilazione.
        PT_MRIValue = 11,
        PT_MRIValue_Indirect = 12,
        PT_Label_comma_pseudoOnly = 13,

        PT_Lable_comma_MRIValue = 14,
        PT_Lable_comma_MRIValue_indirect = 15,

        PT_EMPTYLINE,
        PT_NOTAPATTERN //questo qui è pretty importante
    }

    enum TokenType
    {
        TK_Label,
        TK_MRI,
        TK_RRIorIOI,
        TK_pseudoInstruction,
        TK_value,
        TK_comma,
        TK_indirectAddressing,

        TK_NOTATOKEN
    }

    class Lexer
    {
        private const int numberOfpatterns = 16; //totale dei pattern riconosciuti, non può mai cambiare
        private static TokenType[] pattern_00 = new TokenType[]
        {
            TokenType.TK_RRIorIOI
        };

        private static TokenType[] pattern_01 = new TokenType[]
        {
            TokenType.TK_MRI, TokenType.TK_Label
        };

        private static TokenType[] pattern_02 = new TokenType[]
        {
            TokenType.TK_MRI, TokenType.TK_Label, TokenType.TK_indirectAddressing
        };

        private static TokenType[] pattern_03 = new TokenType[]
        {
            TokenType.TK_Label, TokenType.TK_comma, TokenType.TK_MRI, TokenType.TK_Label
        };
        
        private static TokenType[] pattern_04 = new TokenType[]
        {
            TokenType.TK_Label, TokenType.TK_comma, TokenType.TK_MRI, TokenType.TK_Label, TokenType.TK_indirectAddressing
        };
        
        private static TokenType[] pattern_05 = new TokenType[]
        {
            TokenType.TK_Label, TokenType.TK_comma, TokenType.TK_RRIorIOI
        };

        private static TokenType[] pattern_06 = new TokenType[]
        {
            TokenType.TK_Label, TokenType.TK_comma, TokenType.TK_pseudoInstruction, TokenType.TK_value
        };

        private static TokenType[] pattern_07 = new TokenType[]
        {
            TokenType.TK_pseudoInstruction, TokenType.TK_value
        };

        private static TokenType[] pattern_08 = new TokenType[]
        {
            TokenType.TK_pseudoInstruction
        };

        private static TokenType[] pattern_09 = new TokenType[]
        {
            TokenType.TK_pseudoInstruction, TokenType.TK_Label
        };

        private static TokenType[] pattern_10 = new TokenType[]
        {
            TokenType.TK_Label, TokenType.TK_comma, TokenType.TK_pseudoInstruction, TokenType.TK_Label
        };

        private static TokenType[] pattern_11 = new TokenType[]
        {
            TokenType.TK_MRI, TokenType.TK_value
        };

        private static TokenType[] pattern_12 = new TokenType[]
        {
            TokenType.TK_MRI, TokenType.TK_value, TokenType.TK_indirectAddressing
        };

        private static TokenType[] pattern_13 = new TokenType[]
        {
            TokenType.TK_Label, TokenType.TK_comma, TokenType.TK_pseudoInstruction
        };

        private static TokenType[] pattern_14 = new TokenType[]
        {
            TokenType.TK_Label, TokenType.TK_comma, TokenType.TK_MRI, TokenType.TK_value
        };

        private static TokenType[] pattern_15 = new TokenType[]
        {
            TokenType.TK_Label, TokenType.TK_comma, TokenType.TK_MRI, TokenType.TK_value, TokenType.TK_indirectAddressing
        };

        private static TokenType[][] allPatterns = new TokenType[numberOfpatterns][]
        {
            pattern_00,
            pattern_01,
            pattern_02,
            pattern_03,
            pattern_04,
            pattern_05,
            pattern_06,
            pattern_07,
            pattern_08,
            pattern_09,
            pattern_10,
            pattern_11,
            pattern_12,
            pattern_13,
            pattern_14,
            pattern_15
        };

        private static PatternType[] allPatternsType = new PatternType[numberOfpatterns]
        {
            PatternType.PT_RRIIOIonly,
            PatternType.PT_MRILabel,
            PatternType.PT_MRILabel_Indirect,
            PatternType.PT_Label_comma_MRILabel,
            PatternType.PT_Label_comma_MRILabel_Indirect,
            PatternType.PT_Label_comma_RRIIOI,
            PatternType.PT_Label_comma_Pseudo_Value,
            PatternType.PT_Pseudo_value,
            PatternType.PT_PseudoOnly,
            PatternType.PT_Pseudo_label,
            PatternType.PT_Label_comma_Pseudo_Label,
            PatternType.PT_MRIValue,
            PatternType.PT_MRIValue_Indirect,
            PatternType.PT_Label_comma_pseudoOnly,
            PatternType.PT_Lable_comma_MRIValue,
            PatternType.PT_Lable_comma_MRIValue_indirect
        };

        private static void removeTabs(string[] source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = source[i].Replace("\t", " ");
            }
        }

        public static LineCode[] tokenizeSource(string[] source)
        {
            int lineCount = 1;
            removeTabs(source);

            LineCode[] tokenizedSource = new LineCode[source.Length];

            for(int i = 0; i < tokenizedSource.Length; i++) //per essere sicuro.
            {
                tokenizedSource[i].type = PatternType.PT_EMPTYLINE;
                tokenizedSource[i].pattern = null;
            }

            for(int i = 0; i < source.Length; i++)
            {
                if(source[i].Length == 0 || source[i][0] == '/')
                {
                    lineCount++;
                    continue; //riga vuota o solo commenti
                }

                List<Token> line = new List<Token>();
                List<TokenType> lineType = new List<TokenType>();

                for (int j = 0; j < source[i].Length; j++)
                {
                    string lexem = "";
                    char ch = source[i][j];
                    if(ch == ' ' || ch == '\t')
                    {
                        continue;
                    }

                    else if(ch == '/')
                    {
                        break; //ciò che segue è un commento
                    }

                    else if(ch != ' ' && ch != '\t') // a questo punto devo cercare il più lungo lessema
                    {
                        //lexem += ch;

                        for(int k = j; k < source[i].Length; k++)
                        {
                            if (ch == ',')
                            {
                                lexem += ch;
                                break;
                            }

                            char ch01 = source[i][k];

                            if(source[i][k] != ' ' && source[i][k] != ',' && source[i][k] != '\t')
                            {
                                lexem += source[i][k]; //cerco il lessema più lungo
                                j++;
                            }

                            else
                            {
                                j--; //torno di un carattere indietro perché potrei aver trovato una virgola
                                break; //dovrebbe averlo trovato.
                            }
                        }
                        //arrivato qui il lessema dovrebbe essere pronto, va quindi analizzato
                        Token newToken = tokenizeLexem(lexem);
                        if(newToken.type == TokenType.TK_NOTATOKEN)
                        {
                            Console.WriteLine("something went terriblly wrong!"); //se non è niente è una label, ma non può tornare "notatoken"
                            return null;
                        }
                        
                        line.Add(newToken);
                        lineType.Add(newToken.type);
                        lexem = "";
                        //checkifops
                    }
                }
                //a questo punto aggiungo la linea di codice alla LineCode, ma per farlo devo creare una nuova linecode
                //prima però controllo se il pattern va bene
                //checkifpatternok

                PatternType check = checkIfPatternIsOk(lineType.ToArray());

                if(check == PatternType.PT_NOTAPATTERN)
                {
                    Printf.printCompileErr(lineCount, "Syntax ERROR"); //pattern non riconosciuto, errore di compilazione
                    return null;
                }

                LineCode newLine = new LineCode(line.ToArray(), check);
                tokenizedSource[i] = newLine;
                lineCount++;
            }
            int a = 0; //debug
            return tokenizedSource;
        }

        private static Token tokenizeLexem(string lexem) //bruttino come codice ma "sticazzi"
        {
            Token newToken = new Token();

            newToken.lexem = lexem;
            newToken.type = TokenType.TK_NOTATOKEN;

            if (int.TryParse(lexem, out _) == true)
            {
                //è un value
                newToken.type = TokenType.TK_value;
                return newToken;
            }

            else if(lexem == ",")
            {
                newToken.type = TokenType.TK_comma;
                return newToken;
            }

            else if(lexem == "I")
            {
                newToken.type = TokenType.TK_indirectAddressing;
                return newToken;
            }

            else if (lexem == IOI.INP.ToString() || lexem == IOI.OUT.ToString())
            {
                newToken.type = TokenType.TK_RRIorIOI;
                return newToken;
            }
            
            for(int i = 0; i < instructionAssembler.pseudo.Length; i++)
            {
                if (lexem == instructionAssembler.pseudo[i].ToString())
                {
                    newToken.type = TokenType.TK_pseudoInstruction;
                    return newToken;
                }
            }

            for (int i = 0; i < instructionAssembler.MRIops.Length; i++)
            {
                if (lexem == instructionAssembler.MRIops[i].ToString())
                {
                    newToken.type = TokenType.TK_MRI;
                    return newToken;
                }
            }

            for(int i = 0; i < instructionAssembler.RRIops.Length; i++)
            {
                if(lexem == instructionAssembler.RRIops[i].ToString())
                {
                    newToken.type = TokenType.TK_RRIorIOI;
                    return newToken;
                }

            }


            newToken.type = TokenType.TK_Label; //se non è niente allora è una label
            return newToken;
        }

        private static PatternType checkIfPatternIsOk(TokenType[] line)
        {
            PatternType type = PatternType.PT_NOTAPATTERN;
            for(int i = 0; i < allPatterns.Length; i++)
            {
                if (line.Length != allPatterns[i].Length)
                    continue;
                //if(line == allPatterns[i]) //wut? perché anche se sono uguali non va bene?
                if(line.SequenceEqual(allPatterns[i]))
                {
                    type = allPatternsType[i];
                    break;
                }
            }

            //qui dovrei mettere alcuni controlli
            //per esempio

            return type;
        }

        public static bool isHex(string test)
        {
            bool itIs = true;
            int i = 0;
            if (test[i] == '-')
                i++;

            for (int k = i; i < test.Length; i++)
            {
                if ((test[i] >= '0' && test[i] <= '9') || (test[i] >= 'A' && test[i] <= 'F'))
                    continue;
                else
                {
                    itIs = false;
                    break;
                }
            }

            return itIs;
        }

        public static short parseHex(string lexem)
        {
            short value = 0;
            string uLexem = ""; //lessema senza il negativo
            int i = 0;
            bool isNegative = false;

            if (lexem[i] == '-')
            {
                i++;
                isNegative = true;
            }

            for (int k = i; k < lexem.Length; k++)
            {
                uLexem += lexem[k];
            }

            //da qui ho il valore unsigned, lo parso e poi gli cambio il segno
            value = short.Parse(uLexem, System.Globalization.NumberStyles.HexNumber);
            if (isNegative)
            {
                value = (short)-value;
            }

            return value;

        }
    }

    class Compiler
    {
        //singleton CompilerData
        private static CompilerData compilerData;

        public static CompilerData getCompilerData()
        {
            return compilerData;
        }

        private static u12 LC = new u12(0);
        private static Dictionary<string, u12> labelTabel;
        private static List<string> labelKeys;
        public static void Compile(string[] source)
        {
            if(source == null)
            {
                return;
            }
            labelTabel = new Dictionary<string, u12>();
            labelKeys = new List<string>();
            CompilerData data = new CompilerData();

            LineCode[] tokenizedSource = Lexer.tokenizeSource(source);
            data.tkSource = tokenizedSource;

            if (tokenizedSource != null)
                data.completed = firstStep(tokenizedSource, data);
            
            if (data.completed == true)
                data.completed = secondStep(tokenizedSource, data.machineCode);

            if(data.completed)
            {
                compilerData = data;
                Program.setVMParameters(compilerData.machineCode, compilerData.programAddress);
                Printf.printRamOnBuffer(compilerData);
                Printf.printRegisters(Program.getVm());
            }
        }

        public static bool firstStep(LineCode[] source, CompilerData data)
        {
            int lineCount = 1;
            LC.setValue(0); //durante entramble le passate LC DEVE essere 0, quindi per sicurezza lo setto sempre
            bool pcSetted = false;
            bool firstInstrFound = false;

            for(int i = 0; i < source.Length; i++)
            {
                if (i > 0)
                    lineCount++;

                if (firstInstrFound && !pcSetted)
                {
                    data.programAddress.setValue(LC.getValue());
                    pcSetted = true;
                }

                if (source[i].type != PatternType.PT_EMPTYLINE)
                {
                    firstInstrFound = true;
                }

                if (source[i].type == PatternType.PT_EMPTYLINE)
                {
                    continue;
                }

                else if(source[i].pattern[0].type == TokenType.TK_Label) //ovvero, se il primo elemento della riga è una label
                {
                    labelTabel.Add(source[i].pattern[0].lexem, new u12(LC.getValue()));
                    labelKeys.Add(source[i].pattern[0].lexem);
                }

                else if(source[i].type == PatternType.PT_Pseudo_label)
                {
                    string lexem = source[i].pattern[0].lexem;
                    string label = source[i].pattern[1].lexem;
                    if (lexem == "ORG")
                    {
                        if (!Lexer.isHex(label))
                        {
                            Printf.printCompileErr(lineCount, "you can only use hexadecimal value for ORG");
                            return false;
                        }

                        else
                        {
                            if(Lexer.parseHex(label) < 0)
                            {
                                Printf.printCompileErr(lineCount, "only use positive value for ORG!");
                                return false;
                            }
                            LC.setValue(ushort.Parse(label, System.Globalization.NumberStyles.HexNumber));
                            continue;
                        }
                    }

                    else if(lexem == "END" || lexem == "DEC")
                    {
                        Printf.printCompileErr(lineCount, "syntax ERROR");
                        return false;
                    }
                }

                else if (source[i].type == PatternType.PT_Pseudo_value)
                {
                    string lexem = source[i].pattern[0].lexem;
                    string label = source[i].pattern[1].lexem;
                    if (lexem == "ORG")
                    {
                        if (Lexer.parseHex(label) < 0)
                        {
                            Printf.printCompileErr(lineCount, "only use positive value for ORG!");
                            return false;
                        }
                        LC.setValue(ushort.Parse(label, System.Globalization.NumberStyles.HexNumber)); //andrea: "qui gatta ci cova"
                        continue;
                    }

                }

                else if(source[i].type == PatternType.PT_PseudoOnly)
                {
                    string lexem = source[i].pattern[0].lexem;
                    if (lexem != "END")
                    {
                        Printf.printCompileErr(lineCount, "syntax ERROR, are you missing a value?");
                        return false;
                    }

                    else if(lexem == "END")
                    {
                        break;
                    }
                }

                LC.increment();
            }

            Console.WriteLine("Printing Found Labels with Addresses\n");
            for (int i = 0; i < labelKeys.Count; i++)
            {
                Console.WriteLine(labelKeys[i] + " " + labelTabel[labelKeys[i]].getValue());
            }
            Console.WriteLine("");


            return true;
        }
        
        public static bool secondStep(LineCode[] source, i16[] ram) //Implementazione passaggi assembler delle dispende del prof navarra
        {
            LC.setValue(0);
            int lineCount = 1;

            for(int i = 0; i < source.Length; i++)
            {

                if (i > 0)
                    lineCount++;

                if (source[i].type == PatternType.PT_EMPTYLINE)
                {
                    continue;
                }
                int k = 0;
                //per prima cosa faccio le pseudo istruzioni
                LineCode line = source[i];

                while (k < line.pattern.Length) //in teoria questo funzoina still testing
                {
                    TokenType tp = line.pattern[k].type;
                    if (tp == TokenType.TK_MRI || tp == TokenType.TK_RRIorIOI || tp == TokenType.TK_pseudoInstruction)
                    {
                        break;
                    }

                    k++;
                }

                //END
                if(line.type == PatternType.PT_PseudoOnly || line.type == PatternType.PT_Label_comma_pseudoOnly)
                {
                    string pseudo = line.pattern[k].lexem;
                    if(pseudo == "END")
                    {
                        //finito 
                        break;
                    }

                    else
                    {
                        Printf.printCompileErr(lineCount, "syntax ERROR, are you missing a value?");
                        return false;
                        //in teoria questo passaggio è evitabile perché lo faccio nella prima passata.
                    }
                }

                //ORG DEC HEX
                else if((line.type == PatternType.PT_Pseudo_value || line.type == PatternType.PT_Pseudo_label 
                      || line.type == PatternType.PT_Label_comma_Pseudo_Value || line.type == PatternType.PT_Label_comma_Pseudo_Label))
                {
                    string pseudo = line.pattern[k].lexem; //pseudoistruzione 
                    string value = line.pattern[k + 1].lexem; //valore, che sia esadecimale o decimale
                    
                    if(pseudo == "ORG")
                    {
                        LC.setValue(ushort.Parse(value, System.Globalization.NumberStyles.HexNumber));
                        continue;
                    }

                    else if(pseudo == "DEC")
                    {
                        if(line.type == PatternType.PT_Pseudo_label || line.type == PatternType.PT_Label_comma_Pseudo_Label)
                        {
                            Printf.printCompileErr(lineCount, "you can only use decimal value for DEC");
                            return false;
                        }

                        ram[LC.getValue()].setValue(short.Parse(value)); //valore inserito nella ram
                        ram[LC.getValue()].setInstruction(value);
                        LC.increment();
 
                    }

                    else if(pseudo == "HEX")
                    {
                        if (!Lexer.isHex(value))
                        {
                            Printf.printCompileErr(lineCount, "syntax ERROR");
                            return false;
                        }

                        ram[LC.getValue()].setValue(Lexer.parseHex(value));
                        ram[LC.getValue()].setInstruction(value);
                        LC.increment();
                    }
                    //non faccio altri check perché se ci sono errori vengono intercettati nella prima passata
                }

                // PSEUDO FATTE IN TEORIA, PASSIAMO ALLE ISTRUZIONI
                // RRI/IOI
                else if(line.type == PatternType.PT_RRIIOIonly || line.type == PatternType.PT_Label_comma_RRIIOI)
                {   
                    string lexem = line.pattern[k].lexem;
                    IOI istr = IOI.NOT_IOIISTR;
                    if(lexem == IOI.INP.ToString())
                    {
                        istr = IOI.INP;
                    }

                    else if(lexem == IOI.OUT.ToString())
                    {
                        istr = IOI.OUT;
                    }

                    if(istr != IOI.NOT_IOIISTR)
                    {
                        ram[LC.getValue()].setValue(instructionAssembler.buildIOIop(istr).getValue());
                        ram[LC.getValue()].setInstruction(lexem);
                        LC.increment();
                        
                        continue;
                    }

                    //se il codice iarriva qui allora l'istr non era una IOI
                    RRI instr = instructionAssembler.findRRI(lexem);
                    if(instr == RRI.NOT_INSTR)
                    {
                        Console.WriteLine("something went wrong");
                        return false;
                    }

                    ram[LC.getValue()].setValue(instructionAssembler.buildRRIop(instr).getValue());
                    ram[LC.getValue()].setInstruction(lexem);
                    LC.increment();
                    
                }

                else if(line.type == PatternType.PT_MRIValue || line.type == PatternType.PT_MRIValue_Indirect || 
                        line.type == PatternType.PT_MRILabel || line.type == PatternType.PT_MRILabel_Indirect ||
                        line.type == PatternType.PT_Label_comma_MRILabel || line.type == PatternType.PT_Label_comma_MRILabel_Indirect ||
                        line.type == PatternType.PT_Lable_comma_MRIValue || line.type == PatternType.PT_Lable_comma_MRIValue_indirect)
                {
                    string lexem = line.pattern[k].lexem;
                    string address = line.pattern[k + 1].lexem;
                    u12 addressValue = new u12();
                    ushort addressValue_ = 0;
                    addressing addressType = addressing.direct;


                    //per prima cosa controllo se il valore è una label
                    if(!(labelTabel.TryGetValue(address, out addressValue)))
                    {
                      
                        //if (ushort.TryParse(address, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addressValue_))
                        if(Lexer.isHex(address))
                        {
                            if (Lexer.parseHex(address) < 0)
                            {
                                Printf.printCompileErr(lineCount, "negative value is out of bounds");
                                return false;
                            }
                            //allora è un esadecimale (anche se fosse decimale, comunque lo tratto da esadeciamle
                            addressValue.setValue(addressValue_);
                        }

                        else
                        {
                            //allora è errore
                            Printf.printCompileErr(lineCount, address + " no such Label or value");
                            return false;
                        }
                    }

                    //se si arriva qui allora abbiamo l'indirizzo
                    if (line.type == PatternType.PT_MRILabel_Indirect ||
                       line.type == PatternType.PT_Label_comma_MRILabel_Indirect ||
                       line.type == PatternType.PT_Lable_comma_MRIValue_indirect)
                            addressType = addressing.indirect;

                    ram[LC.getValue()] = instructionAssembler.buildMRIop(instructionAssembler.findMRI(lexem), addressValue, addressType);
                    ram[LC.getValue()].setInstruction(lexem);
                    LC.increment();
                }
            }
            return true;
        }

    }


    class instructionAssembler
    {

        public static MRI[] MRIops = new MRI[]
        {
            MRI.AND,
            MRI.ADD,
            MRI.LDA,
            MRI.STA,
            MRI.BUN,
            MRI.BSA,
            MRI.ISZ,

            MRI.NOT_INSTR
        };

        public static RRI[] RRIops = new RRI[]
        {
            RRI.CLA,
            RRI.CLE,
            RRI.CMA,
            RRI.CME,
            RRI.CIR,
            RRI.CIL,
            RRI.INC,
            RRI.SPA,
            RRI.SNA,
            RRI.SZA,
            RRI.SZE,
            RRI.HLT,

            RRI.NOT_INSTR
        };

        public static IOI[] IOIops = new IOI[]
        {
            IOI.INP,
            IOI.OUT,

            IOI.NOT_IOIISTR
        };

        public static pseudoOPs[] pseudo = new pseudoOPs[]
        {
            pseudoOPs.DEC,
            pseudoOPs.END,
            pseudoOPs.HEX,
            pseudoOPs.ORG
        };

        public static RRI findRRI(string lexem)
        {
            for(int i = 0; i < RRIops.Length; i++)
            {
                if(lexem == RRIops[i].ToString())
                {
                    return RRIops[i];
                }    
            }
            return RRI.NOT_INSTR;
        }

        public static MRI findMRI(string lexem)
        {
            for (int i = 0; i < MRIops.Length; i++)
            {
                if (lexem == MRIops[i].ToString())
                {
                    return MRIops[i];
                }
            }
            return MRI.NOT_INSTR;
        }

        public static i16 buildMRIop(MRI instruction, u12 address, addressing I = addressing.direct)
        {
            i16 reg = new i16();
            short value = 0;

            //inserisco l'opr nei bit 12, 13 e 14 di value
            short OPR = (short)instruction;
            value = (short)(Utility.setBit((ushort)value, 12, Utility.isBitSet(OPR, 0)));
            value = (short)(Utility.setBit((ushort)value, 13, Utility.isBitSet(OPR, 1)));
            value = (short)(Utility.setBit((ushort)value, 14, Utility.isBitSet(OPR, 2)));

            //setto l'indirizzamento indiretto
            value = (short)(Utility.setBit((ushort)value, 15, I == addressing.indirect));

            //inserisco l'address
            value = (short)(value | (short)(address.getValue()));

            reg.setValue(value);

            return reg;
        }

        public static i16 buildRRIop(RRI instruction)
        {
            i16 reg = new i16();
            reg.setValue((short)instruction);

            return reg;
        }

        public static i16 buildIOIop(IOI instruction)
        {
            i16 reg = new i16();
            reg.setValue((short)instruction);

            return reg;
        }
    }
}
