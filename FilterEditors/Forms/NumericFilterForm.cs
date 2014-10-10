using System;
using System.Globalization;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.UI.Resources;
using System.Text;

namespace MainBit.Projections.SimilarContent.FilterEditors.Forms
{
    public class NumericFilterForm : IFormProvider {
        public const string FormName = "NumericFilterSimilarContent";

        private readonly Work<IResourceManager> _resourceManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public NumericFilterForm(IShapeFactory shapeFactory, Work<IResourceManager> resourceManager)
        {
            _resourceManager = resourceManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: FormName,
                        _СomparisonValue: Shape.FieldSet(
                            Id: "fieldset-comparisonvalue",
                            _Max: Shape.TextBox(
                                Id: "comparisonvalue", Name: "ComparisonValue",
                                Title: T("Comparision value"),
                                Classes: new[] { "text medium tokenized" }
                                )
                            ),
                        _OperatorMin: Shape.SelectList(
                            Id: "operatorMin", Name: "OperatorMin",
                            Title: T("Operator for lower limit"),
                            Size: 1,
                            Multiple: false
                            ),
                        _FieldSetMin: Shape.FieldSet(
                            Id: "fieldset-min",
                            _Min: Shape.TextBox(
                                Id: "min", Name: "Min",
                                Title: T("Range to lower limit")
                                //Classes: new[] { "tokenized" }
                                )
                            ),
                        _OperatorMax: Shape.SelectList(
                            Id: "operatorMax", Name: "OperatorMax",
                            Title: T("Operator for upper limit"),
                            Size: 1,
                            Multiple: false
                            ),
                        _FieldSetMax: Shape.FieldSet(
                            Id: "fieldset-max",
                            _Max: Shape.TextBox(
                                Id: "max", Name: "Max",
                                Title: T("Range to upper limit")
                                //Classes: new[] { "tokenized" }
                                )
                            )
                    );

                    f._OperatorMin.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.Ignored), Text = T("Ignored").Text });
                    f._OperatorMin.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.Equals), Text = T("Is equal to").Text });
                    f._OperatorMin.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.GreaterThan), Text = T("Is greater than").Text });
                    f._OperatorMin.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.GreaterThanEquals), Text = T("Is greater than or equal to").Text });

                    f._OperatorMax.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.Ignored), Text = T("Ignored").Text });
                    f._OperatorMax.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.LessThan), Text = T("Is less than").Text });
                    f._OperatorMax.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.LessThanEquals), Text = T("Is less than or equal to").Text });

                    return f;
                };

            context.Form(FormName, form);

        }

        public static Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState, string property) {

            decimal comparisonValue = 0;
            if (!Decimal.TryParse(Convert.ToString(formState.ComparisonValue), out comparisonValue))
            {
                throw new NotNeedApplyFilterException();
                // return null; // return y => y.Or(x1 => x1.IsNull(property), x2 => x2.IsNotNull(property));
            }
            var opMin = (NumericOperator)Enum.Parse(typeof(NumericOperator), Convert.ToString(formState.OperatorMin));
            var opMax = (NumericOperator)Enum.Parse(typeof(NumericOperator), Convert.ToString(formState.OperatorMax));

            decimal min, max;
            Action<IHqlExpressionFactory> minPredicate = null, maxPredicate = null;
            if (opMin != NumericOperator.Ignored)
            {
                min = Decimal.Parse(Convert.ToString(formState.Min), CultureInfo.InvariantCulture);
                minPredicate = GetFilterPredicate(opMin, property, comparisonValue - min);
            }
            if (opMax != NumericOperator.Ignored)
            {
                max = Decimal.Parse(Convert.ToString(formState.Max), CultureInfo.InvariantCulture);
                maxPredicate = GetFilterPredicate(opMax, property, comparisonValue + max);
            }

            if (minPredicate != null && maxPredicate != null)
            {
                return y => y.And(minPredicate, maxPredicate);
            }
            else
            {
                return minPredicate ?? maxPredicate;
            }
        }

        private static Action<IHqlExpressionFactory> GetFilterPredicate(NumericOperator op, string property, decimal value)
        {

            switch (op) {
                case NumericOperator.LessThan:
                    return x => x.Lt(property, value);
                case NumericOperator.LessThanEquals:
                    return x => x.Le(property, value);
                case NumericOperator.Equals:
                    return x => x.Eq(property, value);
                case NumericOperator.GreaterThan:
                    return x => x.Gt(property, value);
                case NumericOperator.GreaterThanEquals:
                    return x => x.Ge(property, value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static LocalizedString DisplayFilter(string fieldName, dynamic formState, Localizer T) {
            
            var opMin = (NumericOperator)Enum.Parse(typeof(NumericOperator), Convert.ToString(formState.OperatorMin));
            var opMax = (NumericOperator)Enum.Parse(typeof(NumericOperator), Convert.ToString(formState.OperatorMax));

            string min = Convert.ToString(formState.Min);
            string max = Convert.ToString(formState.Max);

            var displayFilter = new StringBuilder();
            if (opMin != NumericOperator.Ignored)
            {
                displayFilter.Append("value"); // displayFilter.Append(fieldName);
                displayFilter.Append("-");
                displayFilter.Append(min);
                displayFilter.Append(" ");
                displayFilter.Append(GetSign(opMin));
                displayFilter.Append(" ");
            }
            displayFilter.Append(fieldName);
            if(opMax != NumericOperator.Ignored) {
                displayFilter.Append(" ");
                displayFilter.Append(GetSign(opMax));
                displayFilter.Append(" ");
                displayFilter.Append("value"); // displayFilter.Append(fieldName);
                displayFilter.Append("+");
                displayFilter.Append(max);
            }

            // should never be hit, but fail safe
            return new LocalizedString(displayFilter.ToString());
        }

        private static string GetSign(NumericOperator op)
        {

            switch (op)
            {
                case NumericOperator.LessThan:
                    return "<";
                case NumericOperator.LessThanEquals:
                    return "<=";
                case NumericOperator.Equals:
                    return "=";
                case NumericOperator.GreaterThan:
                    return "<"; // ">"; - depends on the position of the compared values
                case NumericOperator.GreaterThanEquals:
                    return "<="; //">="; - depends on the position of the compared values
                default:
                    return "ERROR";
            }
        }
    }

    public enum NumericOperator
    {
        Ignored,
        LessThan,
        LessThanEquals,
        Equals,
        GreaterThan,
        GreaterThanEquals
    }
}