using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintSpace;
using UtilityStuff;
using Emulatore_Pdp8;
using System.IO;
using System.Runtime.InteropServices;


//siccome i commenti sul pdp8 possono essere aprti ma mai chiusi, allora se trovo un "/" vado a capo boh non lo so aiuto
namespace Assembler
{

    struct Token
    {
        public TokenType type;
        public string lexem;
    }
    
    struct LineCode //qui andrà un puntatore a funzione che serve a sputare il machinecode
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
        private const int numberOfpatterns = 9; //totale dei pattern riconosciuti, non può mai cambiare
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
            pattern_08
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
            PatternType.PT_PseudoOnly
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
                    continue; //riga vuota
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
                    Console.WriteLine("Line " + lineCount + ":" + " Compile error"); //pattern non riconosciuto, errore di compilazione
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
    }

    class Compiler
    {
        private static u12 LC = new u12(0);
        private static Dictionary<string, u12> labelTabel = new Dictionary<string, u12>();
        
        /*public static void Compile(string[] sourceCode)
        {
            int lineCounter = 1;
            string[] source = sourceCode;
            List<string> allLalebl = new List<string>();
            
            bool isEND = false;
            bool commaFound = false;

            removeSpaces(source);

            //prima passata
            string label = "";
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].Length == 0)
                    continue;

                for (int j = 0; j < source[i].Length; j++)
                {
                    if(char.IsDigit(source[i][0]))
                    {
                        Printf.printCompileErr(lineCounter, "you can't use numbers as first Label Character");
                        return;
                    }

                    char ch = source[i][j];

                    if(ch == '/')
                    {
                        Console.WriteLine("Comment");
                        break;
                    }

                    if (ch != ',')
                    {
                        label += ch;
                    }

                    if (label == "ORG")
                    {
                        Console.WriteLine("Found ORG setting LC");
                        string value = "";
                        for(int k = j + 1; k < source[i].Length; k++)
                        {
                            value += source[i][k];
                        }
                        Console.WriteLine(value);

                        if(!OnlyHexInString(value))
                        {
                            Printf.printCompileErr(lineCounter, "you can only use Hexadecimal values for ORG");
                            return;
                        }

                        LC.setValue(ushort.Parse(value, System.Globalization.NumberStyles.HexNumber)); //parse string to hex and setting LC
                        //analizza il valore dopo ORG e setta LC
                        //NB utilizzare dopo ORG qualcosa come ADD, vabene, perché ADD è un valore ammesso in esadecimale e quindi passabile
                    }

                    if (label == "END")
                    {
                        isEND = true;
                        Console.WriteLine("Compiler's first step completed");
                        break;
                    }

                    else if (ch == ',' && !commaFound)
                    {
                        commaFound = true;
                        short isOp = checkIfOp(label);
                        if(isOp != -1)
                        {
                            Printf.printCompileErr(lineCounter, "you can't use an instruction or pseudo-instruction as a Label");
                            return;
                        }

                        string isValid = "";
                        isValid += source[i][j + 1].ToString() + source[i][j + 2].ToString() + source[i][j + 3].ToString();
                        //Console.WriteLine(isValid);

                        isOp = checkIfOp(isValid);
                        if (isValid == "DEC" || isValid == "HEX" || (isOp != -1 && isOp != (short)PseudoInstruction.END && isOp != (short)PseudoInstruction.ORG))
                        {
                            Console.WriteLine("Label!");
                            labelTabel.Add(label, new u12(LC.getValue()));
                            allLalebl.Add(label);                          
                        }

                        else
                        {
                            Printf.printCompileErr(lineCounter, "you can only use <DEC>, <HEX> or an istruction for a label value");
                            return;
                        }

                        //lookForLabel(source);
                    }
                    
                    else if (ch == ',' && commaFound)
                    {
                        Printf.printCompileErr(lineCounter, "only use comma for Label!");
                        return;
                    }

                    if (isEND)
                        break;
                }
                label = "";
                lineCounter++;
                commaFound = false;
                //Console.Write(lineCounter);
                LC.increment();
                //Console.WriteLine(LC.getValue());
            }

            lineCounter = 1;

            for(int i = 0; i < allLalebl.Count; i++) //just printing things
            {
                string actualLabel = allLalebl[i];
                u12 address = labelTabel[actualLabel];
                Console.WriteLine(actualLabel + " " + address.getValue());
            }

            //seconda passata:
            LC.setValue(0);
        }*/

        /*public static short checkIfOp(string label) //verifico se una certa label è una istruzione/pseudo-istruzione
        {
            short ret = -1; //nb nessuna istruzione ha -1 come opr (infatti -1 è 0b_1111_1111_1111_1111)
            
            //check for MRI
            for(int i = 0; i < MRIops.Length; i++)
            {
                if(label == MRIops[i].ToString())
                {
                    ret = (short)MRIops[i];
                    return ret;
                }
            }

            for(int i = 0; i < RRIops.Length; i++)
            {
                if (label == RRIops[i].ToString())
                {
                    ret = (short)RRIops[i];
                    return ret;
                }
            }

            for (int i = 0; i < IOIops.Length; i++)
            {
                if (label == IOIops[i].ToString())
                {
                    ret = (short)IOIops[i];
                    return ret;
                }
            }

            if(label == PseudoInstruction.ORG.ToString() || label == PseudoInstruction.END.ToString())
            {
                if (label == PseudoInstruction.ORG.ToString())
                    ret = (short)PseudoInstruction.ORG;

                else
                    ret = (short)PseudoInstruction.END;

                return ret;
            }

            return ret; 
        }*/
 
        private static void removeSpaces(string[] source) //public static momentaneamente
        {
            for (int i = 0; i < source.Length; i++)
            {
                //if (source[i].Length == 0)
                //continue;
                source[i] = source[i].Replace(" I", "*I");
                source[i] = source[i].Replace("\tI", "*I");
                source[i] = source[i].Replace(" ", "");
                source[i] = source[i].Replace("\t", "");
                source[i] = source[i].Replace("\r", "");
                Console.WriteLine(source[i]);
            }
        }

        private static bool OnlyHexInString(string test) //check se nella stringa ci sono solo valori esadecimali (works)
        {
            // For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
            // https://stackoverflow.com/questions/223832/check-a-string-to-see-if-all-characters-are-hexadecimal-values
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
            MRI.ISZ
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
            RRI.HLT
        };

        public static IOI[] IOIops = new IOI[]
        {
            IOI.INP,
            IOI.OUT
        };

        public static pseudoOPs[] pseudo = new pseudoOPs[]
        {
            pseudoOPs.DEC,
            pseudoOPs.END,
            pseudoOPs.HEX,
            pseudoOPs.ORG
        };
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
