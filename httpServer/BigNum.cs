using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace CS422
{
    //Floating Point number that can grow indefinately.
    //Many floating point numbers without rounding or trunctation
    //Allow you to determine what operations can be done with float and doubles without data loss.
    //Object is immutable
    //Store in base 10

    class BigNum
    {
        private BigInteger _exp;
        private BigInteger _num;
        private bool _undefined;

        public BigInteger Exp
        {
            get { return _exp; }
        }

        public BigInteger Num
        {
            get { return _num; }
        }

        public BigNum()
        {
            BigInteger _exp = new BigInteger(0);
            BigInteger _num = new BigInteger(0);            
        }

        public BigNum(BigInteger num, BigInteger exp)
        {
            _exp = exp;
            _num = num;
        }

        public BigNum(string number)
        {
            //Converts real number string to BigNum
            //Validate the string : throw an 'argument' exception if the following aren't met:
            // String must start with minus sign, number, or decimal
            // No whitespace
            // Null or empty string are not allowed
            // '-' at most at the beginning
            // '.' at most once
            // [0-9]
            //If valid, initialize to that exact number
            BigNumStringConstructor(number);
            _undefined = false;
        }

        private void BigNumStringConstructor(string number)
        {
            if (number == null || number.Contains(" ") || number.Split('.').Length - 1 > 1) throw new ArgumentException();
            if (!number.StartsWith("-") && !number.StartsWith(".") && !Char.IsNumber(number[0])) throw new ArgumentException();
            if (number.Split('-').Length - 1 > 1 && !number.StartsWith("-")) throw new ArgumentException();

            string E = "";
            if (number.Contains("E"))
            {
                //1.79769313486232E+308
                E = number.Split('E').Last<string>();
                number = number.Split('E').First<string>();
            }

            int decimal_position = 1;
            if (number.Contains("."))
            {
                int index = number.IndexOf('.');                
                number = number.Remove(index, 1);
                decimal_position = number.Length - index;
            }

            _num = BigInteger.Parse(number);
            if (decimal_position != 1)
            {
                _exp = decimal_position * -1;
            }
            else
            {
                _exp = decimal_position;
            }

            if (E != "")
            {
                if (E.Contains("-"))
                {
                    try
                    {
                        _exp -= Convert.ToInt32(E.Substring(1)) + 1;
                    }
                    catch
                    {
                        _undefined = true;
                        return;
                    }
                }
                else if (E.Contains("+"))
                {
                    try
                    {
                        _exp += Convert.ToInt32(E.Substring(1)) + 1;
                    }
                    catch
                    {
                        _undefined = true;
                        return;
                    }
                }
                else
                {
                    try
                    {
                        _exp += Convert.ToInt32(E);
                    }
                    catch
                    {
                        _undefined = true;
                        return;
                    }
                }
            }            

            /*
            string E = "";
            if (number.Contains("E"))
            {
                //1.79769313486232E+308
                E = number.Split('E').Last<string>();
                number = number.Split('E').First<string>();                
            }

            //BigInteger final = new BigInteger();
            BigInteger final = new BigInteger(0);
            int neg = 1;

            int j = number.Length - 1;           
            if (number.Contains(".")) j--;
            if (number.Contains("-")) j--;            
            for (int i = 0; i < number.Length; i++)
            {
                if (number[i] == '.') { _exp = (number.Length - i - 1) * -1; }
                else if (number[i] == '-') { neg = -1; }
                else
                {                    
                    //if (number[i] == '0') final += new BigInteger(Math.Pow(10, j));
                    //else final += new BigInteger(Convert.ToInt32(number[i]) * Math.Pow(10, j));                
                    BigInteger val = BigInteger.Parse(number);               
                    int val2 = Convert.ToInt32(number[i].ToString());
                    final += BigInteger.Multiply(val2, val);
                    j--;
                }
                
            }
            if (E != "")
            {
                if (E.Contains("-"))
                {
                    try
                    {
                        _exp -= Convert.ToInt32(E.Substring(1));
                    }
                    catch
                    {
                        _undefined = true;
                        return;
                    }
                }
                else if (E.Contains("+"))
                {
                    try
                    {
                        _exp += Convert.ToInt32(E.Substring(1));
                    }
                    catch
                    {
                        _undefined = true;
                        return;
                    }
                }
            }
            else
            {
                //Invalid String
                //_undefined = true;                
            }

            if (_exp == null || _exp == 0) _exp = 1;
            _num = final * neg;
            */
        }

        public BigNum(double value, bool useDoubleToString)
        {
            // Constructs BigNum from existing double value
            // If double is NaN. +infinity, or -infinity, then BigNum is initialized to undefined value
                //regardless of the value of the useDoubleToString param
            // If useDoubleToString is true, convert a value to a string using ToString and construct the BigNum 
                //using the first constructor
            // If useDoubleToString is false: initialize the BigNum to exact value represented by the double.
            // This is often different from what toString would return for a double.
            // You must parse through the bits in the value and interpret accoridng to the specs in the doc 
            
            if (Double.IsNaN(value) || Double.IsInfinity(value))
            {
                _undefined = true;    
            }        
            else if (useDoubleToString)
            {
                BigNumStringConstructor(value.ToString());
            }
            else
            {
                BigNumStringConstructor(value.ToString());                
            }
        }        
        
        public override string ToString()
        {
            //Num = 429472874
            //Exp = 3
            //Final = 429472874000
            //Num = 429472874
            //Exp = -3
            //Final = 429472.874            
            bool neg = false;
            if (this.IsUndefined()) return "undefined";

            BigInteger i = new BigInteger(0);
            string val = _num.ToString();
            if (val[0] == '-') { neg = true; val = val.Substring(1, val.Length - 1); }
            if (_exp < 0)
            {
                int length = val.Length;
                for (int j = 0; j < (_exp * -1) - length; j++)
                    val = "0" + val;
            
                val = val.Substring(0, (int)(val.Length + _exp)) + "." + val.Substring((int)(val.Length + _exp), val.Length - (int)(val.Length + _exp));                
            }
            else if (_exp > 0)
            {
                for (; i < _exp - 1; i++)
                {
                    val += "0";
                }
            }
            else
                return "0";
            if (neg) val = "-" + val;
            return checkToString(val);


            //Returns the base-10 representation of this number
            //Implement this to use the minimum number of digits needs to accurately represent the number
            // without scientific notation
            // No white space character
            // If the number is an integer, do not include the decimal point at end.
            // if -1 < x < 1, do not include leading '0'
            // return 0 if 0
            // include '-' for all neg #'s, but not '+' for pos #'s
            // No unecessary leading or trailing '0's 
            // If undefined, return "undefined"            
        }

        private string checkToString(string value)
        {
            if (value.Contains('.'))
            {
                int lastNonZero = -1;
                string result = value.Split('.').Last<string>();
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] != '0')
                        lastNonZero = i;
                }

                if (lastNonZero == -1) return value.Split('.').First<string>();

                result = result.Substring(0, lastNonZero + 1);
                return value.Split('.').First<string>() + "." + result;
            }
            return value;
        }

        public bool IsUndefined()
        {
            // Special-case state where it is undefined.
            // have a member var that is true when the number is Undefined. false when not
            // for binary operations - return undefined if either operand is undefined.
            return _undefined;
        }

        public static BigNum operator +(BigNum lhs, BigNum rhs)
        {
            if (lhs.IsUndefined() || rhs.IsUndefined()) return null;

            //Implements addition as a lossless operation
            BigNum return_value = new BigNum();
            BigInteger exp_difference = new BigInteger(0);
            BigInteger i = new BigInteger(0);

            if (lhs._exp > rhs._exp)
            {
                exp_difference = BigInteger.Abs(lhs._exp - rhs._exp);                              
                for (; i < exp_difference; i++)
                {
                    lhs._num *= 10;
                }
            }
            else if(lhs._exp > rhs._exp)
            {
                exp_difference = rhs._exp - lhs._exp;                                
                for (; i < exp_difference; i++)
                {
                    rhs._num *= 10;
                }
            }

            return_value._num = rhs._num + lhs._num;
            return_value._exp = i * -1;
            /*
            for (; i > 0; i--)
            {
                return_value._num /= 10;
                
            }*/
            //return_value._exp = BigInteger.Max(rhs._exp, lhs._exp);

            return return_value;
        }

        public static BigNum operator -(BigNum lhs, BigNum rhs)
        {
            if (lhs.IsUndefined() || rhs.IsUndefined()) return null;
            //Implements subtraction as a lossless operation
            //Implements addition as a lossless operation
            BigNum return_value = new BigNum();
            BigInteger exp_difference = new BigInteger(0);
            BigInteger i = new BigInteger(0);

            if (lhs._exp > rhs._exp)
            {
                exp_difference = lhs._exp - rhs._exp;
                for (; i < exp_difference; i++)
                {
                    lhs._num *= 10;
                }
            }
            else if (lhs._exp > rhs._exp)
            {
                exp_difference = rhs._exp - lhs._exp;
                for (; i < exp_difference; i++)
                {
                    rhs._num *= 10;
                }
            }

            return_value._num = lhs._num - rhs._num;
            for (; i > 0; i--)
            {
                return_value._num /= 10;
            }
            return_value._exp = BigInteger.Max(rhs._exp, lhs._exp);

            return return_value;
        }

        public static BigNum operator *(BigNum lhs, BigNum rhs)
        {
            if (lhs.IsUndefined() || rhs.IsUndefined()) return null;
            //Implements multiplication as a lossless operation
            //Needs to be in O(n) time
            //Don't just add a whole bunch of times

            if (rhs.IsUndefined() || lhs.IsUndefined())
            {
                BigNum h = new BigNum();
                h._undefined = true;
                return h;                
            }

            if (lhs._exp == rhs._exp) return new BigNum(lhs._num * rhs._num, rhs._exp);
            return new BigNum(lhs._num * rhs._num, lhs._exp + rhs._exp); 
        }

        /*
         * Class assignment notes
         * Test not a number from double, int
         * Infinitiy is a value a double can hold
         *          
         */
        public static BigNum operator /(BigNum lhs, BigNum rhs)
        {
            if (lhs.IsUndefined() || rhs.IsUndefined()) return null;
            // division is potentially lossy
            // return undefined is rhs is '0' or either is undefined
            // perform a finite number of iterations and gets a reasonable
            // number of digits before terminates, perfect operation might not be possible
            // Have at least 20 digits by default
            // Need O(N) solution          

            int NUM_PRECISION = 20;
            lhs._num *= new BigInteger(Math.Pow(10,NUM_PRECISION));
            BigInteger num = lhs._num / rhs._num;
            BigInteger exp;
            //if (lhs._exp == rhs._exp) exp = rhs._exp - NUM_PRECISION;
            exp = lhs._exp - rhs._exp - NUM_PRECISION;
            return new BigNum(num, exp);
        }

        public static bool operator >(BigNum lhs, BigNum rhs)
        {
            if (lhs.IsUndefined() || rhs.IsUndefined()) return false;
            if (lhs._exp > rhs._exp) return true;
            else if (lhs._exp == rhs._exp && lhs._num > rhs._num) return true;
            return false;
        }

        public static bool operator >=(BigNum lhs, BigNum rhs)
        {
            if (lhs.IsUndefined() || rhs.IsUndefined()) return false;
            if (lhs._exp > rhs._exp) return true;
            else if (lhs._exp == rhs._exp && lhs._num >= rhs._num) return true;
            return false;
        }

        public static bool operator <(BigNum lhs, BigNum rhs)
        {
            if (lhs.IsUndefined() || rhs.IsUndefined()) return false;
            if (lhs._exp < rhs._exp) return true;
            else if (lhs._exp == rhs._exp && lhs._num < rhs._num) return true;
            return false;
        }

        public static bool operator <=(BigNum lhs, BigNum rhs)
        {
            if (lhs.IsUndefined() || rhs.IsUndefined()) return false;
            if (lhs._exp < rhs._exp) return true;
            else if (lhs._exp == rhs._exp && lhs._num <= rhs._num) return true;
            return false;
        }

        public static bool IsToStringCorrect(double value)
        {
            //Utility function that determines whether or not ToString
            //for the given value generates the exact represenation of the 
            //stored value
            BigNum n1 = new BigNum(value, true);
            BigNum n2 = new BigNum(value, true);
            return n1.ToString() == n2.ToString(); 
        }

    }
}