from setuptools import setup
from setuptools.dist import Distribution


class BinaryDistribution(Distribution):
    def has_ext_modules(self):
        return True  # forces a platform-specific wheel tag (win_amd64)


setup(distclass=BinaryDistribution)
