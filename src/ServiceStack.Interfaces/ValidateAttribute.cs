using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Caching;
using ServiceStack.DataAnnotations;

namespace ServiceStack
{
    /// <summary>
    /// Assert pre-conditions before DTO's Fluent Validation properties are evaluated
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    [Tag("PropertyOrder")]
    public class ValidateRequestAttribute : AttributeBase, IValidateRule
    {
        public ValidateRequestAttribute() {}
        public ValidateRequestAttribute(string validator) => Validator = validator;
        
        /// <summary>
        /// Script Expression to create an IPropertyValidator registered in Validators.Types
        /// </summary>
        public string Validator { get; set; }

        /// <summary>
        /// Boolean #Script Code Expression to Test
        /// ARGS:
        ///   - Request: IRequest
        ///   -     dto: Request DTO
        ///   -   field: Property Name
        ///   -      it: Property Value
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Combine multiple conditions
        /// </summary>
        [Ignore]
        public string[] Conditions
        {
            get => new []{ Condition };
            set => Condition = ValidateAttribute.Combine("&&", value);
        }

        /// <summary>
        /// Custom ErrorCode to return 
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Custom Error Message to return
        ///  - {PropertyName}
        ///  - {PropertyValue}
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Custom Status Code to return when invalid
        /// </summary>
        public int StatusCode { get; set; }
        
        [Ignore]
        public string[] AllConditions
        {
            get => throw new NotSupportedException(nameof(AllConditions));
            set => Condition = ValidateAttribute.Combine("&&", value);
        }

        [Ignore]
        public string[] AnyConditions
        {
            get => throw new NotSupportedException(nameof(AnyConditions));
            set => Condition = ValidateAttribute.Combine("||", value);
        }
        
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ValidateAttribute : AttributeBase, IValidateRule
    {
        public ValidateAttribute() {}
        public ValidateAttribute(string validator) => Validator = validator;
        
        /// <summary>
        /// Script Expression to create an IPropertyValidator registered in Validators.Types
        /// </summary>
        public string Validator { get; set; }

        /// <summary>
        /// Boolean #Script Code Expression to Test
        /// ARGS:
        ///   - Request: IRequest
        ///   -     dto: Request DTO
        ///   -   field: Property Name
        ///   -      it: Property Value
        /// </summary>
        public string Condition { get; set; }

        [Ignore]
        public string[] AllConditions
        {
            get => throw new NotSupportedException(nameof(AllConditions));
            set => Condition = Combine("&&", value);
        }

        [Ignore]
        public string[] AnyConditions
        {
            get => throw new NotSupportedException(nameof(AnyConditions));
            set => Condition = Combine("||", value);
        }

        /// <summary>
        /// Custom ErrorCode to return 
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Refer to FluentValidation docs for Variable
        ///  - {PropertyName}
        ///  - {PropertyValue}
        /// </summary>
        public string Message { get; set; }

        public static string Combine(string comparand, params string[] conditions)
        {
            var sb = new StringBuilder();
            var joiner = ") " + comparand + " ("; 
            foreach (var condition in conditions)
            {
                if (string.IsNullOrEmpty(condition))
                    continue;
                if (sb.Length > 0)
                    sb.Append(joiner);
                sb.Append(condition);
            }

            sb.Insert(0, '(');
            sb.Append(')');
            return sb.ToString();
        }
    }

    public interface IValidateRule
    {
        string Validator { get; set; }
        string Condition { get; set; }
        string ErrorCode { get; set; }
        string Message { get; set; }
    }

    public class ValidateRuleBase : IValidateRule 
    {
        public string Validator { get; set; }
        public string Condition { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
    }

    public interface IValidationSource
    {
        IEnumerable<KeyValuePair<string, IValidateRule>> GetValidationRules(Type type);
    }

    public interface IValidationSourceWriter
    {
        void SaveValidationRules(List<ValidateRule> validateRules);
    }

    /// <summary>
    /// Data persistence Model 
    /// </summary>
    public class ValidateRule : ValidateRuleBase
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        /// <summary>
        /// The name of the Type 
        /// </summary>
        [Required]
        public string Type { get; set; }
        
        /// <summary>
        /// The property field for Property Validators, null for Type Validators 
        /// </summary>
        public string Field { get; set; }
        
        /// <summary>
        /// Results sorted in ascending SortOrder, Id
        /// </summary>
        public int SortOrder { get; set; }
    }
}