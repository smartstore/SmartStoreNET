using SmartStore.Core.Domain.Orders;
using System;

namespace SmartStore.Services.Customers
{
    public class CustomerInformation
    {

        public DateTime? RegistrationFrom
        {
            get; set;
        }

        public DateTime? RegistrationTo
        {
            get; set;
        }

        public int [] CustomerRoleIds
        {
            get; set;
        }

        public string Email
        {
            get; set;
        }

        public string Username
        {
            get; set;
        }

        public string FirstName
        {
            get; set;
        }

        public string LastName
        {
            get; set;
        }

        public int DayOfBirth
        {
            get; set;
        }

        public int MonthOfBirth
        {
            get; set;
        }
        public string Company
        {
            get; set;
        }
        public string Phone
        {
            get; set;
        }
        public string ZipPostalCode
        {
            get; set;
        }
        public bool LoadOnlyWithShoppingCart
        {
            get; set;
        }
        public ShoppingCartType? ShoppingCartType
        {
            get; set;
        }
        public int PageIndex
        {
            get; set;
        }
        public int PageSize
        {
            get; set;
        }

        public class Builder
        {
            readonly CustomerInformation _customerInformation; 

            public Builder ()
            {
                _customerInformation = new CustomerInformation();
            }

            public Builder SetRegistrationTo(DateTime? rTo)
            {
                _customerInformation.RegistrationTo = rTo;
                return this;
            }

            public Builder SetCustomerRoleIds (int[] cId)
            {
                _customerInformation.CustomerRoleIds = cId;
                return this;
            }

            public Builder SetEmail (string e)
            {
                _customerInformation.Email = e;
                return this;
            }

            public Builder SetUsername(string user)
            {
                _customerInformation.Username = user;
                return this;
            }

            public Builder SetFirstName(string first)
            {
                _customerInformation.FirstName = first;
                return this;
            }

            public Builder SetLastName(string last)
            {
                _customerInformation.LastName = last;
                return this;
            }

            public Builder SetDayOfBirth(int dayB)
            {
                _customerInformation.DayOfBirth = dayB;
                return this;
            }

            public Builder SetMonthOfBirth(int mnthB)
            {
                _customerInformation.MonthOfBirth = mnthB;
                return this;
            }
            public Builder SetCompany(string comp)
            {
                _customerInformation.Company = comp;
                return this;
            }
            public Builder SetPhone(string p)
            {
                _customerInformation.Phone = p;
                return this;
            }
            public Builder SetZipPostalCode(string zip)
            {
                _customerInformation.ZipPostalCode = zip;
                return this;
            }
            public Builder SetLoadOnlyWithShoppingCart(bool ShoppingCart)
            {
                _customerInformation.LoadOnlyWithShoppingCart = ShoppingCart;
                return this;
            }
            public Builder SetSct(ShoppingCartType? st)
            {
                _customerInformation.ShoppingCartType = st;
                return this;
            }
            public Builder SetPageIndex(int pI)
            {
                _customerInformation.PageIndex = pI;
                return this;
            }
            public Builder SetPageSize(int pS)
            {
                _customerInformation.PageSize = pS;
                return this;
            }

            public CustomerInformation Build()
            {
                return _customerInformation;
            }
        }
    }
}
