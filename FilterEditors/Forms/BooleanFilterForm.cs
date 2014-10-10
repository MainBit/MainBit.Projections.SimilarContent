using System;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System.Web.Mvc;
using System.Globalization;

namespace MainBit.Projections.SimilarContent.FilterEditors.Forms {
    public class BooleanFilterForm : IFormProvider {
        public const string FormName = "BooleanFilterSimilarContent";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public BooleanFilterForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        _СomparisonValue: Shape.FieldSet(
                            Id: "fieldset-comparisonvalue",
                            _Max: Shape.TextBox(
                                Id: "comparisonvalue", Name: "ComparisonValue",
                                Title: T("Comparision value"),
                                Classes: new[] { "text medium tokenized" }
                                )
                            ),
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                            ),
                        _OperatorUndefined: Shape.SelectList(
                            Id: "operatorUndefined", Name: "OperatorUndefined",
                            Title: T("Filter by value if comparision value undefined"),
                            Size: 1,
                            Multiple: false
                            )
                    );

                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(BooleanOperator.Equals), Text = T("Is equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(BooleanOperator.NotEquals), Text = T("Is not equal to").Text });

                    f._OperatorUndefined.Add(new SelectListItem { Value = Convert.ToString(BooleanUndefinedOperator.Any), Text = T("Any").Text });
                    f._OperatorUndefined.Add(new SelectListItem { Value = Convert.ToString(BooleanUndefinedOperator.Undefined), Text = T("Undefined").Text });
                    f._OperatorUndefined.Add(new SelectListItem { Value = Convert.ToString(BooleanUndefinedOperator.True), Text = T("True").Text });
                    f._OperatorUndefined.Add(new SelectListItem { Value = Convert.ToString(BooleanUndefinedOperator.False), Text = T("False").Text });

                    return f;
                };

            context.Form(FormName, form);

        }

        public static LocalizedString DisplayFilter(string fieldName, dynamic formState, Localizer T) {

            var comparisonValue = Convert.ToString(formState.ComparisonValue);
            var op = (BooleanOperator)Enum.Parse(typeof(BooleanOperator), Convert.ToString(formState.Operator));
            var opUndef = (BooleanUndefinedOperator)Enum.Parse(typeof(BooleanUndefinedOperator), Convert.ToString(formState.OperatorUndefined));

            string display;
            if (op == BooleanOperator.Equals)
            {
                display = "{0} equals {1}";
            }
            else
            {
                display = "{0} is not equal to {1}";
            }

            switch (opUndef)
            {
                case BooleanUndefinedOperator.Any:
                    display += ", igonre undefined";
                    break;
                case BooleanUndefinedOperator.Undefined:
                    break;
                case BooleanUndefinedOperator.True:
                    display += ", select true if undefined";
                    break;
                case BooleanUndefinedOperator.False:
                    display += ", select false if undefined";
                    break;
                default:
                    break;
            }

            return T(display, fieldName, comparisonValue);
        }

        public static Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState, string property) {

            var comparisonValue = Convert.ToString(formState.ComparisonValue);
            var op = (BooleanOperator)Enum.Parse(typeof(BooleanOperator), Convert.ToString(formState.Operator));
            var opUndef = (BooleanUndefinedOperator)Enum.Parse(typeof(BooleanUndefinedOperator), Convert.ToString(formState.OperatorUndefined));

            if (string.IsNullOrWhiteSpace(comparisonValue))
            {
                switch (opUndef)
                {
                    case BooleanUndefinedOperator.Any:
                        throw new NotNeedApplyFilterException(); //return null; // return x => x.Or(x1 => x1.IsNotNull(property), x2 => x2.IsNull(property));
                    case BooleanUndefinedOperator.Undefined:
                        return x => x.IsNull(property);
                    case BooleanUndefinedOperator.True:
                        return x => x.Gt(property, (long)0);
                    case BooleanUndefinedOperator.False:
                        return x => x.Eq(property, (long)0);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                var comparisonValueBoolean = Convert.ToBoolean(comparisonValue);
                if ((comparisonValueBoolean && op == BooleanOperator.Equals) || (!comparisonValueBoolean && op == BooleanOperator.NotEquals))
                {
                    return x => x.Gt(property, (long)0);
                }
                else
                {
                    return x => x.Eq(property, (long)0);
                }
            }
        }
    }

    public enum BooleanOperator
    {
        Equals,
        NotEquals
    }

    public enum BooleanUndefinedOperator
    {
        Any,
        Undefined,
        True,
        False
    }
}