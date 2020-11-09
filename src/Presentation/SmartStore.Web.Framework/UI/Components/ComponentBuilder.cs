using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
    public abstract class ComponentBuilder<TComponent, TBuilder, TModel> : IHtmlString, IHideObjectMembers
        where TComponent : Component
        where TBuilder : ComponentBuilder<TComponent, TBuilder, TModel>
    {
        private ComponentRenderer<TComponent> _renderer;

        protected ComponentBuilder(TComponent component, HtmlHelper<TModel> htmlHelper)
        {
            Guard.NotNull(component, nameof(component));
            Guard.NotNull(htmlHelper, nameof(htmlHelper));

            this.Component = component;
            this.HtmlHelper = htmlHelper;
        }

        protected internal HtmlHelper<TModel> HtmlHelper
        {
            get;
            private set;
        }

        protected internal TComponent Component
        {
            get;
            private set;
        }

        public TComponent ToComponent()
        {
            return this.Component;
        }

        protected ComponentRenderer<TComponent> Renderer
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = EngineContext.Current.ContainerManager.Resolve<ComponentRenderer<TComponent>>();
                    EnrichRenderer(_renderer);
                }
                return _renderer;
            }
            private set
            {
                _renderer = value;
                if (_renderer != null)
                {
                    EnrichRenderer(_renderer);
                }
            }
        }

        private void EnrichRenderer(ComponentRenderer<TComponent> renderer)
        {
            renderer.Component = this.Component;
            renderer.HtmlHelper = this.HtmlHelper;
            renderer.ViewContext = this.HtmlHelper.ViewContext;
            renderer.ViewData = this.HtmlHelper.ViewData;
        }

        public TBuilder WithRenderer(ComponentRenderer<TComponent> instance)
        {
            Guard.NotNull(instance, nameof(instance));

            return this.WithRenderer<ComponentRenderer<TComponent>>(instance);
        }

        public TBuilder WithRenderer<T>(ComponentRenderer<TComponent> instance)
            where T : ComponentRenderer<TComponent>
        {
            Guard.NotNull(instance, nameof(instance));

            this.Renderer = instance;
            return this as TBuilder;
        }

        public TBuilder WithRenderer<T>()
            where T : ComponentRenderer<TComponent>
        {
            return this.WithRenderer(typeof(T));
        }

        public TBuilder WithRenderer(Type rendererType)
        {
            Guard.NotNull(rendererType, nameof(rendererType));
            Guard.Implements<ComponentRenderer<TComponent>>(rendererType);

            if (Activator.CreateInstance(rendererType) is ComponentRenderer<TComponent> renderer)
            {
                this.Renderer = renderer;
            }

            return this as TBuilder;
        }

        public virtual TBuilder Name(string name)
        {
            this.Component.Name = name;
            return this as TBuilder;
        }

        public virtual TBuilder HtmlAttributes(object attributes)
        {
            return this.HtmlAttributes(CommonHelper.ObjectToDictionary(attributes));
        }

        public virtual TBuilder HtmlAttributes(IDictionary<string, object> attributes)
        {
            this.Component.HtmlAttributes.Merge(attributes);
            return this as TBuilder;
        }

        public virtual TBuilder HtmlAttribute(string name, object value)
        {
            Guard.NotEmpty(name, nameof(name));
            Guard.NotNull(value, nameof(value));

            this.Component.HtmlAttributes[name] = value;
            return this as TBuilder;
        }

        public virtual TBuilder AddCssClass(string cssClass)
        {
            Guard.NotEmpty(cssClass, nameof(cssClass));

            this.Component.HtmlAttributes.AppendCssClass(cssClass);
            return this as TBuilder;
        }

        public string ToHtmlString()
        {
            return this.Renderer.ToHtmlString();
        }

        public override string ToString()
        {
            return this.ToHtmlString();
        }

        public virtual void Render()
        {
            this.Renderer.Render();
        }

        public static implicit operator TComponent(ComponentBuilder<TComponent, TBuilder, TModel> builder)
        {
            return builder.ToComponent();
        }
    }
}
