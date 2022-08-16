import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  Svg: React.ComponentType<React.ComponentProps<'svg'>>;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Work together',
    Svg: require('@site/static/img/opensource.svg').default,
    description: (
      <>
        Brass is open source which means that everyone can contribute to it!
      </>
    ),
  },
  {
    title: 'Easy to Use',
    Svg: require('@site/static/img/simple.svg').default,
    description: (
      <>
        Brass is really simple and easy to use web framework for C#.
      </>
    ),
  },
  {
    title: 'Create web APIs',
    Svg: require('@site/static/img/api.svg').default,
    description: (
      <>
        Brass is great tool for creating web APIs
      </>
    ),
  },
];

function Feature({title, Svg, description}: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
